using Microsoft.AspNetCore.Authorization;

namespace AMYL.Api.Web.Authorization
{
    public class PermissionAuthorizeAttribute :
        AuthorizeAttribute, IAuthorizationRequirement, IAuthorizationRequirementData
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
}
