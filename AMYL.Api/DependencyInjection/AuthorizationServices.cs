using AMYL.Api.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace AMYL.Api.DependencyInjection;

public static class AuthorizationServices
{
    public static IServiceCollection AddApiAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        return services;
    }
}
