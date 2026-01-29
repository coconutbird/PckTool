namespace PckTool.Abstractions.Batch;

/// <summary>
///     Represents a single action to be performed in a batch project.
/// </summary>
public interface IProjectAction
{
    /// <summary>
    ///     Gets the type of action to perform.
    /// </summary>
    ProjectActionType ActionType { get; }

    /// <summary>
    ///     Gets or sets an optional description for this action.
    /// </summary>
    string? Description { get; set; }

    /// <summary>
    ///     Validates that this action is properly configured.
    /// </summary>
    /// <returns>A result indicating whether the action is valid.</returns>
    ActionValidationResult Validate();
}
