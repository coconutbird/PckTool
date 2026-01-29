namespace PckTool.Abstractions.Batch;

/// <summary>
///     Represents the result of validating a batch project.
/// </summary>
public sealed class BatchProjectValidationResult
{
    private BatchProjectValidationResult(bool isValid, IReadOnlyList<string> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    /// <summary>
    ///     Gets a value indicating whether the project is valid.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    ///     Gets the list of validation errors.
    /// </summary>
    public IReadOnlyList<string> Errors { get; }

    /// <summary>
    ///     Creates a successful validation result.
    /// </summary>
    public static BatchProjectValidationResult Success()
    {
        return new BatchProjectValidationResult(true, Array.Empty<string>());
    }

    /// <summary>
    ///     Creates a failed validation result with error messages.
    /// </summary>
    /// <param name="errors">The list of validation errors.</param>
    public static BatchProjectValidationResult Failure(IEnumerable<string> errors)
    {
        return new BatchProjectValidationResult(false, errors.ToList().AsReadOnly());
    }

    /// <summary>
    ///     Creates a failed validation result with a single error message.
    /// </summary>
    /// <param name="error">The validation error.</param>
    public static BatchProjectValidationResult Failure(string error)
    {
        return new BatchProjectValidationResult(false, new[] { error });
    }
}
