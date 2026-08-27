using AMYL.Api.Domain.Common;

namespace AMYL.Api.Shared.Extensions;

/// <summary>
/// The only place an application <see cref="Result"/> is translated into an
/// HTTP response. Error categories never carry status codes themselves.
/// </summary>
public static class ResultExtensions
{
    public static IResult HandleResult(Result result) =>
        result.IsSuccess
            ? Results.Ok()
            : ToProblem(result.Error!);

    public static IResult HandleResult<T>(Result<T> result) =>
        result.IsSuccess
            ? Results.Ok(result.Value)
            : ToProblem(result.Error!);

    private static IResult ToProblem(Error error)
    {
        if (error.Type is ErrorType.Validation && error.ValidationErrors is not null)
        {
            return Results.ValidationProblem(
                error.ValidationErrors,
                statusCode: StatusCodes.Status400BadRequest,
                title: error.Description);
        }

        var statusCode = error.Type switch
        {
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };

        return Results.Problem(
            statusCode: statusCode,
            title: GetTitle(error.Type),
            detail: error.Description,
            extensions: new Dictionary<string, object?>
            {
                ["code"] = error.Code
            });
    }

    private static string GetTitle(ErrorType errorType) => errorType switch
    {
        ErrorType.Unauthorized => "Unauthorized",
        ErrorType.Forbidden => "Forbidden",
        ErrorType.NotFound => "Not Found",
        ErrorType.Conflict => "Conflict",
        _ => "Bad Request"
    };
}
