using AMYL.Api.Web.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AMYL.Api.Features.Authentication.Login;

public static class LoginCommandEndpoint
{
    public static void MapLoginCommand(this RouteGroupBuilder group)
    {
        group.MapPost("/login", async (
            [FromBody] LoginCommand command,
            ISender sender,
            CancellationToken cancellationToken, HttpResponse Response) =>
        {
            var result = await sender.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                CookiesExtensions.SetTCookies(Response, result.Value!.AccessToken, result.Value.RefreshToken);
            }

            return ResultExtensions.HandleResult(result);
        });
    }
}
