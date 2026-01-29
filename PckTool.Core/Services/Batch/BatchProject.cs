using System.Text.Json;
using System.Text.Json.Serialization;

using PckTool.Abstractions.Batch;
using PckTool.Core.Games;

namespace PckTool.Core.Services.Batch;

/// <summary>
///     Represents a batch project that defines operations to perform on Wwise audio files.
/// </summary>
public sealed class BatchProject : IBatchProject
{
    /// <summary>
    ///     Current schema version for batch project files.
    /// </summary>
    public const int CurrentSchemaVersion = 1;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new ProjectActionConverter(), new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    ///     Gets or sets the project file path (set when loaded/saved).
    /// </summary>
    [JsonIgnore] public string? FilePath { get; private set; }

    /// <inheritdoc />
    [JsonPropertyName("schemaVersion")] public int SchemaVersion { get; set; } = CurrentSchemaVersion;

    /// <inheritdoc />
    [JsonPropertyName("name")] public string Name { get; set; } = "Untitled Batch Project";

    /// <inheritdoc />
    [JsonPropertyName("description")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }

    /// <summary>
    ///     Gets or sets whether to skip updating HIRC size references when replacing WEM files.
    ///     Default is false (HIRC sizes are updated automatically).
    /// </summary>
    [JsonPropertyName("skipHircSizeUpdates")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool SkipHircSizeUpdates { get; set; }

    /// <summary>
    ///     Gets or sets the game identifier (e.g., "hw2", "hwde").
    ///     Stored as a string in JSON for readability and forward compatibility.
    /// </summary>
    [JsonPropertyName("game")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Game { get; set; }

    /// <summary>
    ///     Gets or sets an optional override path to the game installation directory.
    ///     If null, the tool will attempt to auto-detect the game path.
    /// </summary>
    [JsonPropertyName("gamePath")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? GamePath { get; set; }

    /// <inheritdoc />
    [JsonPropertyName("inputFiles")] public IList<string> InputFiles { get; set; } = new List<string>();

    /// <inheritdoc />
    [JsonPropertyName("outputDirectory")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? OutputDirectory { get; set; }

    /// <inheritdoc />
    [JsonPropertyName("actions")] public IList<IProjectAction> Actions { get; set; } = new List<IProjectAction>();

    /// <summary>
    ///     Gets or sets custom notes for the project.
    /// </summary>
    [JsonPropertyName("notes")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Notes { get; set; }

    /// <summary>
    ///     Gets the parsed game enum value from the <see cref="Game" /> string.
    /// </summary>
    [JsonIgnore] public SupportedGame ParsedGame => SupportedGameExtensions.ParseGame(Game);

    /// <summary>
    ///     Gets the game metadata for this project, or null if the game is unknown.
    /// </summary>
    [JsonIgnore] public GameMetadata? GameMetadata => GameMetadata.GetMetadata(ParsedGame);

    /// <inheritdoc />
    public BatchProjectValidationResult Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
        {
            errors.Add("Project name is required.");
        }

        if (InputFiles.Count == 0)
        {
            errors.Add("At least one input file is required.");
        }
        else
        {
            var basePath = GetBasePath();

            for (var i = 0; i < InputFiles.Count; i++)
            {
                var inputFile = InputFiles[i];
                var fullPath = Path.IsPathRooted(inputFile)
                    ? inputFile
                    : Path.Combine(basePath, inputFile);

                if (!File.Exists(fullPath))
                {
                    errors.Add($"Input file not found: {inputFile}");
                }
            }
        }

        if (Actions.Count == 0)
        {
            errors.Add("At least one action is required.");
        }
        else
        {
            for (var i = 0; i < Actions.Count; i++)
            {
                var result = Actions[i].Validate();

                if (!result.IsValid)
                {
                    errors.Add($"Action {i + 1}: {result.ErrorMessage}");
                }
            }
        }

        return errors.Count > 0
            ? BatchProjectValidationResult.Failure(errors)
            : BatchProjectValidationResult.Success();
    }

    /// <summary>
    ///     Creates a new batch project with the specified name.
    /// </summary>
    /// <param name="name">The project name.</param>
    /// <returns>A new batch project instance.</returns>
    public static BatchProject Create(string name = "Untitled Batch Project")
    {
        return new BatchProject { Name = name };
    }

    /// <summary>
    ///     Loads a batch project from a JSON file.
    /// </summary>
    /// <param name="path">The path to the project file.</param>
    /// <returns>The loaded project, or null if loading failed.</returns>
    public static BatchProject? Load(string path)
    {
        try
        {
            using var stream = File.OpenRead(path);
            var project = Load(stream);

            if (project is not null)
            {
                project.FilePath = path;
            }

            return project;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     Loads a batch project from a stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <returns>The loaded project, or null if loading failed.</returns>
    public static BatchProject? Load(Stream stream)
    {
        try
        {
            return JsonSerializer.Deserialize<BatchProject>(stream, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     Saves the project to a JSON file.
    /// </summary>
    /// <param name="path">The path to save to. If null, uses the FilePath property.</param>
    /// <returns>True if saving succeeded, false otherwise.</returns>
    public bool Save(string? path = null)
    {
        path ??= FilePath;

        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        try
        {
            var directory = Path.GetDirectoryName(path);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var stream = File.Create(path);
            Save(stream);
            FilePath = path;

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     Saves the project to a stream.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    public void Save(Stream stream)
    {
        JsonSerializer.Serialize(stream, this, JsonOptions);
    }

    /// <summary>
    ///     Gets the base path for resolving relative paths.
    /// </summary>
    /// <returns>The directory containing the project file, or the current directory.</returns>
    public string GetBasePath()
    {
        if (!string.IsNullOrEmpty(FilePath))
        {
            var directory = Path.GetDirectoryName(FilePath);

            if (!string.IsNullOrEmpty(directory))
            {
                return directory;
            }
        }

        return Environment.CurrentDirectory;
    }

    /// <summary>
    ///     Adds a replace WEM action to the project.
    /// </summary>
    /// <param name="targetId">The target WEM ID to replace.</param>
    /// <param name="sourcePath">The path to the replacement file.</param>
    /// <param name="description">An optional description.</param>
    /// <returns>This project for fluent chaining.</returns>
    public BatchProject AddReplaceWem(uint targetId, string sourcePath, string? description = null)
    {
        Actions.Add(
            new ReplaceAction
            {
                TargetType = TargetType.Wem, TargetId = targetId, SourcePath = sourcePath, Description = description
            });

        return this;
    }

    /// <summary>
    ///     Adds a replace BNK action to the project.
    /// </summary>
    /// <param name="targetId">The target BNK ID to replace.</param>
    /// <param name="sourcePath">The path to the replacement file.</param>
    /// <param name="description">An optional description.</param>
    /// <returns>This project for fluent chaining.</returns>
    public BatchProject AddReplaceBnk(uint targetId, string sourcePath, string? description = null)
    {
        Actions.Add(
            new ReplaceAction
            {
                TargetType = TargetType.Bnk, TargetId = targetId, SourcePath = sourcePath, Description = description
            });

        return this;
    }

    /// <summary>
    ///     Adds an input file to the project.
    /// </summary>
    /// <param name="path">The path to the input file.</param>
    /// <returns>This project for fluent chaining.</returns>
    public BatchProject AddInputFile(string path)
    {
        InputFiles.Add(path);

        return this;
    }
}
