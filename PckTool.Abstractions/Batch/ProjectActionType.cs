namespace PckTool.Abstractions.Batch;

/// <summary>
///     Defines the type of action to perform in a batch project.
/// </summary>
public enum ProjectActionType
{
    /// <summary>
    ///     Replace an existing WEM or BNK with new data.
    /// </summary>
    Replace,

    /// <summary>
    ///     Add a new WEM or BNK (placeholder for future implementation).
    /// </summary>
    Add,

    /// <summary>
    ///     Remove an existing WEM or BNK (placeholder for future implementation).
    /// </summary>
    Remove
}
