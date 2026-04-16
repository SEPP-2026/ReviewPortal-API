namespace ReviewPortal.Application.Common;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden
}

public sealed class Result<T>
{
    public bool IsSuccess { get; }

    public T? Value { get; }

    public string? Error { get; }

    public ErrorType? FailureType { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
    }

    private Result(string error, ErrorType errorType)
    {
        IsSuccess = false;
        Error = error;
        FailureType = errorType;
    }

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(string error, ErrorType errorType = global::ReviewPortal.Application.Common.ErrorType.Validation) => new(error, errorType);

    public static Result<T> NotFound(string error) => new(error, global::ReviewPortal.Application.Common.ErrorType.NotFound);

    public static Result<T> Conflict(string error) => new(error, global::ReviewPortal.Application.Common.ErrorType.Conflict);

    public static Result<T> Unauthorized(string error) => new(error, global::ReviewPortal.Application.Common.ErrorType.Unauthorized);

    public static Result<T> Forbidden(string error) => new(error, global::ReviewPortal.Application.Common.ErrorType.Forbidden);
}
