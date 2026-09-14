namespace AMYL.Api.Infrastructure.RateLimiting;

/// <summary>
/// Policy names, consumed by the endpoints that attach them. They live here rather than
/// beside the registration because a slice must not reference the DI folder.
/// </summary>
public static class RateLimitPolicies
{
    public const string General = nameof(General);
    public const string Login = nameof(Login);
}
