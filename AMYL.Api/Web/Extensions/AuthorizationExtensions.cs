using AMYL.Api.Web.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace AMYL.Api.Web.Extensions
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddApiAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization();
            services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
            return services;
        }
    }
}
