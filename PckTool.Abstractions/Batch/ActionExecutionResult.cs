namespace PckTool.Abstractions.Batch;

/// <summary>
///     Represents the result of executing a project action.
/// </summary>
public sealed class ActionExecutionResult
{
    private ActionExecutionResult(bool success, string message, IProjectAction action)
    {
        Success = success;
        Message = message;
        Action = action;
    }

    /// <summary>
    ///     Gets a value indicating whether the action executed successfully.
    /// </summary>
    public bool Success { get; }

    /// <summary>
    ///     Gets a message describing the result of the action.
    /// </summary>
    public string Message { get; }

    /// <summary>
    ///     Gets the action that was executed.
    /// </summary>
    public IProjectAction Action { get; }

    /// <summary>
    ///     Gets or sets additional details about the execution result.
    /// </summary>
    public WemReplacementResult? WemResult { get; init; }

    /// <summary>
    ///     Creates a successful execution result.
    /// </summary>
    /// <param name="action">The action that was executed.</param>
    /// <param name="message">A message describing the successful execution.</param>
    /// <param name="wemResult">Optional WEM replacement result details.</param>
    public static ActionExecutionResult Succeeded(
        IProjectAction action,
        string message,
        WemReplacementResult? wemResult = null)
    {
        return new ActionExecutionResult(true, message, action) { WemResult = wemResult };
    }

    /// <summary>
    ///     Creates a failed execution result.
    /// </summary>
    /// <param name="action">The action that failed.</param>
    /// <param name="message">A message describing the failure.</param>
    public static ActionExecutionResult Failed(IProjectAction action, string message)
    {
        return new ActionExecutionResult(false, message, action);
    }
}
