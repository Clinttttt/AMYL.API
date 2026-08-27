using AMYL.Api.Shared.Security;
using AMYL.Api.Web.Authorization;
using AMYL.Api.Web.Extensions;
using AMYL.Api.Web.RateLimiting;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AMYL.Api.Features.Memories.Create;

public static class CreateMemoryCommandEndpoint
{
    public static void MapCreateMemoryCommand(this RouteGroupBuilder group)
    {
        group.MapPost("/create", async (
            [FromBody] CreateMemoryCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return ResultExtensions.HandleResult(result);
        })
        .RequireRateLimiting(RateLimitPolicies.General)
        .RequireAuthorization(new PermissionAuthorizeAttribute(Modules.Memories, Permissions.Create));
    }
}
