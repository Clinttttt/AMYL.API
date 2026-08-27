using AMYL.Api.Shared.Security;
using Microsoft.AspNetCore.Authorization;

namespace AMYL.Api.Web.Authorization
{
    public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionAuthorizeAttribute>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionAuthorizeAttribute requirement)
        {
            bool HasPermissioon = context.User.HasClaim(
                c => c.Type == CustomClaimTypes.Permission && c.Value == requirement.RequiredPermission);

            if (HasPermissioon)
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
