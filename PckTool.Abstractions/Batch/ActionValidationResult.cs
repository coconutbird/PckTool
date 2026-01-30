namespace PckTool.Abstractions.Batch;

/// <summary>
///     Represents the result of validating a project action.
/// </summary>
public sealed class ActionValidationResult
{
    private ActionValidationResult(bool isValid, string? errorMessage = null)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    ///     Gets a value indicating whether the action is valid.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    ///     Gets the error message if the action is invalid.
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    ///     Creates a successful validation result.
    /// </summary>
    public static ActionValidationResult Success()
    {
        return new ActionValidationResult(true);
    }

    /// <summary>
    ///     Creates a failed validation result with an error message.
    /// </summary>
    /// <param name="errorMessage">The error message describing why validation failed.</param>
    public static ActionValidationResult Failure(string errorMessage)
    {
        return new ActionValidationResult(false, errorMessage);
    }
}
