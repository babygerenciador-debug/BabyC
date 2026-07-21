namespace FleetOS.Shared.Results;

/// <summary>
/// Represents the result of an operation, containing either a success value or an error.
/// Used to avoid using exceptions for business logic flow control.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("Cannot create a successful result with an error.");
        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("Cannot create a failure result without an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);
    public static Result<TValue> Failure<TValue>(Error error) => new(default!, false, error);
}

/// <summary>Generic typed result.</summary>
public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public TValue? Value => IsSuccess
        ? _value
        : throw new InvalidOperationException("Cannot access value of a failure result.");

    public static implicit operator Result<TValue>(TValue value) => Success(value);
}
