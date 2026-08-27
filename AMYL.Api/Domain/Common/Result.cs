namespace AMYL.Api.Domain.Common;

public enum ErrorType
{
    Failure,
    Validation,
    Unauthorized,
    Forbidden,
    NotFound,
    Conflict
}

public sealed record Error(
    ErrorType Type,
    string Code,
    string Description,
    IReadOnlyDictionary<string, string[]>? ValidationErrors = null);

public class Result
{
    protected Result(bool isSuccess, Error? error)
    {
        if (isSuccess == (error is not null))
        {
            throw new ArgumentException("A successful result cannot contain an error, and a failed result must contain one.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public Error? Error { get; }

    public static Result Success() => new(true, null);

    public static Result Failure(string description) =>
        new(false, new Error(ErrorType.Failure, "request.failure", description));

    public static Result Unauthorized(string description = "Authentication is required.") =>
        new(false, new Error(ErrorType.Unauthorized, "authentication.required", description));

    public static Result Forbidden(string description = "You do not have permission to perform this action.") =>
        new(false, new Error(ErrorType.Forbidden, "authorization.forbidden", description));

    public static Result NotFound(string description) =>
        new(false, new Error(ErrorType.NotFound, "resource.not_found", description));

    public static Result BadRequest(string description) =>
        new(false, new Error(ErrorType.Failure, "request.invalid", description));

    public static Result Conflict(string description) =>
        new(false, new Error(ErrorType.Conflict, "resource.conflict", description));

    public static Result ValidationFailure(IReadOnlyDictionary<string, string[]> errors) =>
        new(false, new Error(
            ErrorType.Validation,
            "validation.failed",
            "One or more validation errors occurred.",
            errors));
}

public sealed class Result<T> : Result
{
    private Result(T? value, bool isSuccess, Error? error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public T? Value { get; }

    public static Result<T> Success(T value) => new(value, true, null);

    public static implicit operator Result<T>(T value) => Success(value);

    public new static Result<T> Failure(string description) =>
        new(default, false, new Error(ErrorType.Failure, "request.failure", description));

    public new static Result<T> Unauthorized(string description = "Authentication is required.") =>
        new(default, false, new Error(ErrorType.Unauthorized, "authentication.required", description));

    public new static Result<T> Forbidden(string description = "You do not have permission to perform this action.") =>
        new(default, false, new Error(ErrorType.Forbidden, "authorization.forbidden", description));

    public new static Result<T> NotFound(string description) =>
        new(default, false, new Error(ErrorType.NotFound, "resource.not_found", description));

    public new static Result<T> Conflict(string description) =>
        new(default, false, new Error(ErrorType.Conflict, "resource.conflict", description));

    public new static Result<T> ValidationFailure(IReadOnlyDictionary<string, string[]> errors) =>
        new(default, false, new Error(
            ErrorType.Validation,
            "validation.failed",
            "One or more validation errors occurred.",
            errors));
}
