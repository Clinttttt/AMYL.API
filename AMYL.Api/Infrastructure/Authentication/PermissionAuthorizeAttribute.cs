using Microsoft.AspNetCore.Authorization;

namespace AMYL.Api.Infrastructure.Authentication;

public class PermissionAuthorizeAttribute
    : AuthorizeAttribute, IAuthorizationRequirement, IAuthorizationRequirementData
{
    public PermissionAuthorizeAttribute(string module, string permission)
    {
        RequiredPermission = $"{module}.{permission}";
    }

    public string RequiredPermission { get; }

    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return this;
    }
}

public sealed class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionAuthorizeAttribute>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionAuthorizeAttribute requirement)
    {
        var hasPermission = context.User.HasClaim(
            claim => claim.Type == CustomClaimTypes.Permission
                && claim.Value == requirement.RequiredPermission);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
