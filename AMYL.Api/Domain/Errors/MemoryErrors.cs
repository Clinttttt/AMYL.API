using AMYL.Api.Domain.Common;

namespace AMYL.Api.Domain.Errors;

public static class MemoryErrors
{
    public static readonly Error NotFound = new(
        ErrorType.NotFound,
        "memory.not_found",
        "Memory not found.");

    public static readonly Error Unauthorized = new(
        ErrorType.Unauthorized,
        "memory.unauthorized",
        "Authentication is required.");

    public static Error UploadFailed(string mediaName) => new(
        ErrorType.Failure,
        "memory.upload_failed",
        $"{mediaName} could not be stored.");
}
