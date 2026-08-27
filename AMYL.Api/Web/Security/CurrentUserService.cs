using AMYL.Api.Shared.Security;
using System.Globalization;
using System.Security.Claims;

namespace AMYL.Api.Web.Security
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }
        public string? UserId => httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        public string? Name => httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.Name);
    }
}
