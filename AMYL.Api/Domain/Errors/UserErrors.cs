using AMYL.Api.Domain.Common;

namespace AMYL.Api.Domain.Errors;

/// <summary>
/// Every user-facing failure this feature can return. Codes live here so a slice
/// cannot typo one, and every code the API emits is greppable in one file.
/// </summary>
public static class UserErrors
{
    public static readonly Error NotFound = new(
        ErrorType.NotFound,
        "user.not_found",
        "User not found.");

    public static readonly Error InvalidCredentials = new(
        ErrorType.Unauthorized,
        "user.invalid_credentials",
        "The username or password is incorrect.");

    public static readonly Error DuplicateUserName = new(
        ErrorType.Conflict,
        "user.duplicate_username",
        "That username or email is already registered.");
}
