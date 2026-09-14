using Microsoft.AspNetCore.Identity;

namespace AMYL.Api.Features.Authentication;

/// <summary>
/// Registers the providers this feature owns. Routing is no longer listed here —
/// each slice's <c>Endpoint</c> is discovered by assembly scan.
/// </summary>
public static class AuthenticationModule
{
    public static IServiceCollection AddAuthenticationFeature(this IServiceCollection services)
    {
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<PasswordHasher<object>>();
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();

        return services;
    }
}
