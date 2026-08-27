using AMYL.Api.Shared.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace AMYL.Api.Shared.Extensions;

public static class AuthorizationServices
{
    public static IServiceCollection AddApiAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        return services;
    }
}
