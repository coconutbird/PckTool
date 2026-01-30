namespace PckTool.Abstractions.Batch;

/// <summary>
///     Represents the result of executing a batch project.
/// </summary>
public sealed class BatchExecutionResult
{
    /// <summary>
    ///     Gets or sets the project that was executed.
    /// </summary>
    public required IBatchProject Project { get; init; }

    /// <summary>
    ///     Gets or sets the list of action execution results.
    /// </summary>
    public required IReadOnlyList<ActionExecutionResult> ActionResults { get; init; }

    /// <summary>
    ///     Gets a value indicating whether all actions executed successfully.
    /// </summary>
    public bool AllSucceeded => ActionResults.All(r => r.Success);

    /// <summary>
    ///     Gets the number of successful actions.
    /// </summary>
    public int SuccessCount => ActionResults.Count(r => r.Success);

    /// <summary>
    ///     Gets the number of failed actions.
    /// </summary>
    public int FailureCount => ActionResults.Count(r => !r.Success);

    /// <summary>
    ///     Gets the total number of actions executed.
    /// </summary>
    public int TotalActions => ActionResults.Count;

    /// <summary>
    ///     Gets a summary of the execution result.
    /// </summary>
    public string Summary => $"Executed {TotalActions} actions: {SuccessCount} succeeded, {FailureCount} failed";
}
