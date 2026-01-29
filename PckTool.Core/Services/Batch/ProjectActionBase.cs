using System.ComponentModel;
using System.Text.Json.Serialization;

using PckTool.Abstractions.Batch;

namespace PckTool.Core.Services.Batch;

/// <summary>
///     Base class for project actions that provides common functionality.
/// </summary>
public abstract class ProjectActionBase : IProjectAction
{
    /// <inheritdoc />
    [JsonPropertyName("action")] public abstract ProjectActionType ActionType { get; }

    /// <inheritdoc />
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Optional description for this action.")]
    public string? Description { get; set; }

    /// <inheritdoc />
    public abstract ActionValidationResult Validate();
}
