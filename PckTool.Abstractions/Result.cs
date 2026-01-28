using System.Diagnostics.CodeAnalysis;

namespace PckTool.Abstractions;

/// <summary>
///     Represents the result of an operation that may succeed or fail.
/// </summary>
/// <typeparam name="T">The type of the value on success.</typeparam>
public readonly struct Result<T>
{
    private readonly T? _value;

    private Result(T value)
    {
        _value = value;
        Error = null;
        IsSuccess = true;
    }

    private Result(string error)
    {
        _value = default;
        Error = error;
        IsSuccess = false;
    }

    /// <summary>
    ///     Gets a value indicating whether the operation succeeded.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))] [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    /// <summary>
    ///     Gets the value if the operation succeeded.
    /// </summary>
    /// <exception cref="InvalidOperationException">The operation failed.</exception>
    public T Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException($"Cannot access Value when operation failed: {Error}");

    /// <summary>
    ///     Gets the error message if the operation failed.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    ///     Creates a successful result with the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>A successful result.</returns>
    public static Result<T> Success(T value)
    {
        return new Result<T>(value);
    }

    /// <summary>
    ///     Creates a failed result with the specified error message.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>A failed result.</returns>
    public static Result<T> Failure(string error)
    {
        return new Result<T>(error);
    }

    /// <summary>
    ///     Implicitly converts a value to a successful result.
    /// </summary>
    public static implicit operator Result<T>(T value)
    {
        return Success(value);
    }

    /// <summary>
    ///     Matches on the result, executing the appropriate function.
    /// </summary>
    /// <typeparam name="TResult">The return type.</typeparam>
    /// <param name="onSuccess">Function to execute on success.</param>
    /// <param name="onFailure">Function to execute on failure.</param>
    /// <returns>The result of the executed function.</returns>
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(_value!) : onFailure(Error!);
    }

    /// <summary>
    ///     Tries to get the value.
    /// </summary>
    /// <param name="value">The value if successful.</param>
    /// <returns>true if successful; otherwise, false.</returns>
    public bool TryGetValue([NotNullWhen(true)] out T? value)
    {
        value = _value;

        return IsSuccess;
    }
}

/// <summary>
///     Represents the result of an operation that may succeed or fail without a value.
/// </summary>
public readonly struct Result
{
    private Result(bool success, string? error = null)
    {
        IsSuccess = success;
        Error = error;
    }

    /// <summary>
    ///     Gets a value indicating whether the operation succeeded.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    /// <summary>
    ///     Gets the error message if the operation failed.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    ///     Creates a successful result.
    /// </summary>
    public static Result Success()
    {
        return new Result(true);
    }

    /// <summary>
    ///     Creates a failed result with the specified error message.
    /// </summary>
    /// <param name="error">The error message.</param>
    public static Result Failure(string error)
    {
        return new Result(false, error);
    }

    /// <summary>
    ///     Matches on the result, executing the appropriate action.
    /// </summary>
    public TResult Match<TResult>(Func<TResult> onSuccess, Func<string, TResult> onFailure)
    {
        return IsSuccess ? onSuccess() : onFailure(Error!);
    }
}
