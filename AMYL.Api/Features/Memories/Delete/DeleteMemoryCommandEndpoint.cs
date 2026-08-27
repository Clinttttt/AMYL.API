using AMYL.Api.Shared.Security;
using AMYL.Api.Web.Authorization;
using AMYL.Api.Web.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AMYL.Api.Features.Memories.Delete;

public static class DeleteMemoryCommandEndpoint
{
    public static void MapDeleteMemoryCommand(this RouteGroupBuilder group)
    {
        group.MapDelete("/delete", async (
            [FromBody] DeleteMemoryCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return ResultExtensions.HandleResult(result);
        })
        .RequireAuthorization(new PermissionAuthorizeAttribute(Modules.Memories, Permissions.Delete));
    }
}
