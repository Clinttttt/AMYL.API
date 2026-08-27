using AMYL.Api.Web.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AMYL.Api.Features.Authentication.Register;

public static class RegisterCommandEndpoint
{
    public static void MapRegisterCommand(this RouteGroupBuilder group)
    {
        group.MapPost("/register", async (
            [FromBody] RegisterCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return ResultExtensions.HandleResult(result);
        });
    }
}
