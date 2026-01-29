using PckTool.Abstractions;
using PckTool.Abstractions.Batch;

namespace PckTool.Core.Services.Batch;

/// <summary>
///     Executes batch project operations on Wwise audio files.
/// </summary>
public sealed class BatchProjectExecutor
{
    private readonly IPckFileFactory _pckFileFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BatchProjectExecutor" /> class.
    /// </summary>
    /// <param name="pckFileFactory">The factory for loading PCK files.</param>
    public BatchProjectExecutor(IPckFileFactory pckFileFactory)
    {
        _pckFileFactory = pckFileFactory ?? throw new ArgumentNullException(nameof(pckFileFactory));
    }

    /// <summary>
    ///     Event raised when an action starts executing.
    /// </summary>
    public event EventHandler<ActionExecutionEventArgs>? ActionStarted;

    /// <summary>
    ///     Event raised when an action completes executing.
    /// </summary>
    public event EventHandler<ActionExecutionEventArgs>? ActionCompleted;

    /// <summary>
    ///     Executes all actions in the batch project.
    /// </summary>
    /// <param name="project">The batch project to execute.</param>
    /// <param name="dryRun">If true, validates without making changes.</param>
    /// <returns>The execution result.</returns>
    public BatchExecutionResult Execute(BatchProject project, bool dryRun = false)
    {
        var results = new List<ActionExecutionResult>();
        var basePath = project.GetBasePath();

        // Load all input files
        var loadedFiles = new Dictionary<string, IPckFile>();

        try
        {
            foreach (var inputFile in project.InputFiles)
            {
                var fullPath = Path.IsPathRooted(inputFile)
                    ? inputFile
                    : Path.Combine(basePath, inputFile);

                var pck = _pckFileFactory.Load(fullPath);
                loadedFiles[inputFile] = pck;
            }

            // Execute each action
            for (var i = 0; i < project.Actions.Count; i++)
            {
                var action = project.Actions[i];
                OnActionStarted(action, i);

                var result = ExecuteAction(project, action, loadedFiles, basePath, dryRun);
                results.Add(result);

                OnActionCompleted(action, i, result);
            }

            // Save modified files if not dry run
            if (!dryRun)
            {
                SaveModifiedFiles(project, loadedFiles, basePath);
            }
        }
        finally
        {
            // Dispose all loaded files
            foreach (var pck in loadedFiles.Values)
            {
                pck.Dispose();
            }
        }

        return new BatchExecutionResult { Project = project, ActionResults = results.AsReadOnly() };
    }

    private ActionExecutionResult ExecuteAction(
        BatchProject project,
        IProjectAction action,
        Dictionary<string, IPckFile> loadedFiles,
        string basePath,
        bool dryRun)
    {
        return action switch
        {
            ReplaceAction replaceAction => ExecuteReplaceAction(project, replaceAction, loadedFiles, basePath, dryRun),
            AddAction => ActionExecutionResult.Failed(action, "Add action is not yet implemented."),
            RemoveAction => ActionExecutionResult.Failed(action, "Remove action is not yet implemented."),
            _ => ActionExecutionResult.Failed(action, $"Unknown action type: {action.GetType().Name}")
        };
    }

    private ActionExecutionResult ExecuteReplaceAction(
        BatchProject project,
        ReplaceAction action,
        Dictionary<string, IPckFile> loadedFiles,
        string basePath,
        bool dryRun)
    {
        // Validate the action
        var validation = action.ValidateWithBasePath(basePath);

        if (!validation.IsValid)
        {
            return ActionExecutionResult.Failed(action, validation.ErrorMessage!);
        }

        if (dryRun)
        {
            return ActionExecutionResult.Succeeded(
                action,
                $"[DRY RUN] Would replace {action.TargetType} 0x{action.TargetId:X8}");
        }

        // Load replacement data
        var sourcePath = action.GetFullSourcePath(basePath);
        var replacementData = File.ReadAllBytes(sourcePath);

        // Determine whether to update HIRC sizes (automatic unless globally disabled)
        var updateHircSizes = !project.SkipHircSizeUpdates;

        // Apply to all loaded files
        var totalReplacements = 0;
        WemReplacementResult? lastResult = null;

        foreach (var pck in loadedFiles.Values)
        {
            if (action.TargetType == TargetType.Wem)
            {
                if (action.TargetBank.HasValue)
                {
                    // Replace WEM only in a specific soundbank
                    var replaced = ReplaceWemInBank(
                        pck,
                        action.TargetBank.Value,
                        action.TargetId,
                        replacementData,
                        updateHircSizes);

                    if (replaced)
                    {
                        totalReplacements++;
                        lastResult = new WemReplacementResult { SourceId = action.TargetId, EmbeddedBanksModified = 1 };
                    }
                }
                else
                {
                    // Replace WEM in all soundbanks
                    var result = pck.ReplaceWem(action.TargetId, replacementData, updateHircSizes);

                    if (result.WasReplaced)
                    {
                        totalReplacements++;
                        lastResult = result;
                    }
                }
            }
            else if (action.TargetType == TargetType.Bnk)
            {
                // BNK replacement - find and replace the soundbank
                var entry = pck.SoundBanks[action.TargetId];

                if (entry is not null)
                {
                    entry.ReplaceWith(replacementData);
                    totalReplacements++;
                }
            }
        }

        if (totalReplacements == 0)
        {
            return ActionExecutionResult.Failed(
                action,
                $"{action.TargetType} 0x{action.TargetId:X8} not found in any input file.");
        }

        var message = $"Replaced {action.TargetType} 0x{action.TargetId:X8} in {totalReplacements} file(s).";

        return ActionExecutionResult.Succeeded(action, message, lastResult);
    }

    /// <summary>
    ///     Replaces a WEM in a specific soundbank only.
    /// </summary>
    private static bool ReplaceWemInBank(
        IPckFile pck,
        uint bankId,
        uint wemId,
        byte[] data,
        bool updateHircSizes)
    {
        var bankEntry = pck.SoundBanks[bankId];

        if (bankEntry is null)
        {
            return false;
        }

        var bank = bankEntry.Parse();

        if (bank is null || !bank.ContainsMedia(wemId))
        {
            return false;
        }

        bank.ReplaceWem(wemId, data, updateHircSizes);
        bankEntry.ReplaceWith(bank);

        return true;
    }

    private static void SaveModifiedFiles(
        BatchProject project,
        Dictionary<string, IPckFile> loadedFiles,
        string basePath)
    {
        var outputDir = project.OutputDirectory;

        foreach (var (inputFile, pck) in loadedFiles)
        {
            if (!pck.HasModifications)
            {
                continue;
            }

            string outputPath;

            if (!string.IsNullOrEmpty(outputDir))
            {
                var outputDirFull = Path.IsPathRooted(outputDir)
                    ? outputDir
                    : Path.Combine(basePath, outputDir);

                if (!Directory.Exists(outputDirFull))
                {
                    Directory.CreateDirectory(outputDirFull);
                }

                var fileName = Path.GetFileName(inputFile);
                outputPath = Path.Combine(outputDirFull, fileName);
            }
            else
            {
                // Save to same location with _modified suffix
                var inputPath = Path.IsPathRooted(inputFile)
                    ? inputFile
                    : Path.Combine(basePath, inputFile);

                var dir = Path.GetDirectoryName(inputPath) ?? basePath;
                var name = Path.GetFileNameWithoutExtension(inputPath);
                var ext = Path.GetExtension(inputPath);
                outputPath = Path.Combine(dir, $"{name}_modified{ext}");
            }

            pck.Save(outputPath);
        }
    }

    private void OnActionStarted(IProjectAction action, int index)
    {
        ActionStarted?.Invoke(this, new ActionExecutionEventArgs(action, index));
    }

    private void OnActionCompleted(IProjectAction action, int index, ActionExecutionResult result)
    {
        ActionCompleted?.Invoke(this, new ActionExecutionEventArgs(action, index, result));
    }
}

/// <summary>
///     Event arguments for action execution events.
/// </summary>
public sealed class ActionExecutionEventArgs : EventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ActionExecutionEventArgs" /> class.
    /// </summary>
    /// <param name="action">The action being executed.</param>
    /// <param name="index">The index of the action in the project.</param>
    /// <param name="result">The result of execution, if completed.</param>
    public ActionExecutionEventArgs(IProjectAction action, int index, ActionExecutionResult? result = null)
    {
        Action = action;
        Index = index;
        Result = result;
    }

    /// <summary>
    ///     Gets the action being executed.
    /// </summary>
    public IProjectAction Action { get; }

    /// <summary>
    ///     Gets the index of the action in the project.
    /// </summary>
    public int Index { get; }

    /// <summary>
    ///     Gets the result of execution, if completed.
    /// </summary>
    public ActionExecutionResult? Result { get; }
}
