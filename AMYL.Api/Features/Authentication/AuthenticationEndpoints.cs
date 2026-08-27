using AMYL.Api.Features.Authentication.Login;
using AMYL.Api.Features.Authentication.Register;

namespace AMYL.Api.Features.Authentication;

public static class AuthenticationEndpoints
{
    public static void MapAuthenticationEndpoints(this WebApplication application)
    {
        var group = application
            .MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapRegisterCommand();
        group.MapLoginCommand();
    }
}
