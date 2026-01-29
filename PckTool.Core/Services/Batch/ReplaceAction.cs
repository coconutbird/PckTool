using System.Text.Json.Serialization;

using PckTool.Abstractions.Batch;

namespace PckTool.Core.Services.Batch;

/// <summary>
///     Represents an action to replace a WEM or BNK with new data.
/// </summary>
public sealed class ReplaceAction : ProjectActionBase
{
    /// <inheritdoc />
    [JsonPropertyName("action")] public override ProjectActionType ActionType => ProjectActionType.Replace;

    /// <summary>
    ///     Gets or sets the type of target (WEM or BNK).
    /// </summary>
    [JsonPropertyName("targetType")] public TargetType TargetType { get; set; } = TargetType.Wem;

    /// <summary>
    ///     Gets or sets the target ID to replace.
    /// </summary>
    [JsonPropertyName("targetId")] public uint TargetId { get; set; }

    /// <summary>
    ///     Gets or sets the path to the source file containing replacement data.
    ///     Path is relative to the project file location.
    /// </summary>
    [JsonPropertyName("sourcePath")] public string? SourcePath { get; set; }

    /// <summary>
    ///     Gets or sets an optional target bank ID to limit WEM replacement to a specific soundbank.
    ///     If null, the WEM is replaced in all soundbanks that contain it.
    ///     Only applicable when TargetType is Wem.
    /// </summary>
    [JsonPropertyName("targetBank")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public uint? TargetBank { get; set; }

    /// <inheritdoc />
    public override ActionValidationResult Validate()
    {
        if (TargetId == 0)
        {
            return ActionValidationResult.Failure("Target ID cannot be 0.");
        }

        if (string.IsNullOrWhiteSpace(SourcePath))
        {
            return ActionValidationResult.Failure("Source path is required for replace action.");
        }

        return ActionValidationResult.Success();
    }

    /// <summary>
    ///     Validates that the source file exists relative to the given base path.
    /// </summary>
    /// <param name="basePath">The base path to resolve relative paths against.</param>
    /// <returns>A validation result.</returns>
    public ActionValidationResult ValidateWithBasePath(string basePath)
    {
        var basicValidation = Validate();

        if (!basicValidation.IsValid)
        {
            return basicValidation;
        }

        var fullPath = Path.IsPathRooted(SourcePath!)
            ? SourcePath!
            : Path.Combine(basePath, SourcePath!);

        if (!File.Exists(fullPath))
        {
            return ActionValidationResult.Failure($"Source file not found: {fullPath}");
        }

        return ActionValidationResult.Success();
    }

    /// <summary>
    ///     Gets the full path to the source file.
    /// </summary>
    /// <param name="basePath">The base path to resolve relative paths against.</param>
    /// <returns>The full path to the source file.</returns>
    public string GetFullSourcePath(string basePath)
    {
        if (string.IsNullOrWhiteSpace(SourcePath))
        {
            throw new InvalidOperationException("Source path is not set.");
        }

        return Path.IsPathRooted(SourcePath)
            ? SourcePath
            : Path.Combine(basePath, SourcePath);
    }
}
