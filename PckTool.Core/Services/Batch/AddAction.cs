using System.Text.Json.Serialization;

using PckTool.Abstractions.Batch;

namespace PckTool.Core.Services.Batch;

/// <summary>
///     Represents an action to add a new WEM or BNK.
///     This is a placeholder for future implementation.
/// </summary>
public sealed class AddAction : ProjectActionBase
{
    /// <inheritdoc />
    [JsonPropertyName("action")] public override ProjectActionType ActionType => ProjectActionType.Add;

    /// <summary>
    ///     Gets or sets the type of target to add (WEM or BNK).
    /// </summary>
    [JsonPropertyName("targetType")] public TargetType TargetType { get; set; } = TargetType.Wem;

    /// <summary>
    ///     Gets or sets the ID for the new entry.
    /// </summary>
    [JsonPropertyName("targetId")] public uint TargetId { get; set; }

    /// <summary>
    ///     Gets or sets the path to the source file to add.
    ///     Path is relative to the project file location.
    /// </summary>
    [JsonPropertyName("sourcePath")] public string? SourcePath { get; set; }

    /// <inheritdoc />
    public override ActionValidationResult Validate()
    {
        if (TargetId == 0)
        {
            return ActionValidationResult.Failure("Target ID cannot be 0.");
        }

        if (string.IsNullOrWhiteSpace(SourcePath))
        {
            return ActionValidationResult.Failure("Source path is required for add action.");
        }

        return ActionValidationResult.Success();
    }
}
