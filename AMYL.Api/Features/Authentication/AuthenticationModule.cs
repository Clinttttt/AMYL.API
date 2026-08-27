using Microsoft.AspNetCore.Identity;

namespace AMYL.Api.Features.Authentication;

public static class AuthenticationModule
{
    /// <summary>
    /// Registers the providers this feature owns.
    /// </summary>
    public static IServiceCollection AddAuthenticationFeature(this IServiceCollection services)
    {
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<PasswordHasher<object>>();
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();

        return services;
    }

    public static void MapAuthentication(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/auth")
            .WithTags("Auth");

        Register.Map(group);
        Login.Map(group);
    }
}
