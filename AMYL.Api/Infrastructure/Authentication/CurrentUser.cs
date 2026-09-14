using System.Security.Claims;

namespace AMYL.Api.Infrastructure.Authentication;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Name { get; }
}

/// <summary>
/// The single place that reads identity claims from <see cref="HttpContext"/>.
/// Handlers must still scope data access to this user; endpoint authorization
/// alone does not enforce row ownership.
/// </summary>
public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor)
    : ICurrentUserService
{
    public string? UserId =>
        httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? Name =>
        httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);
}
