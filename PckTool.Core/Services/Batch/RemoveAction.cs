using System.Text.Json.Serialization;

using PckTool.Abstractions.Batch;

namespace PckTool.Core.Services.Batch;

/// <summary>
///     Represents an action to remove a WEM or BNK.
///     This is a placeholder for future implementation.
/// </summary>
public sealed class RemoveAction : ProjectActionBase
{
    /// <inheritdoc />
    [JsonPropertyName("action")] public override ProjectActionType ActionType => ProjectActionType.Remove;

    /// <summary>
    ///     Gets or sets the type of target to remove (WEM or BNK).
    /// </summary>
    [JsonPropertyName("targetType")] public TargetType TargetType { get; set; } = TargetType.Wem;

    /// <summary>
    ///     Gets or sets the target ID to remove.
    /// </summary>
    [JsonPropertyName("targetId")] public uint TargetId { get; set; }

    /// <inheritdoc />
    public override ActionValidationResult Validate()
    {
        if (TargetId == 0)
        {
            return ActionValidationResult.Failure("Target ID cannot be 0.");
        }

        return ActionValidationResult.Success();
    }
}
