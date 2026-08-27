using AMYL.Api.Shared.Security;
using AMYL.Api.Web.Authorization;
using AMYL.Api.Web.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AMYL.Api.Features.Memories.Update;

public static class UpdateMemoryCommandEndpoint
{
    public static void MapUpdateMemoryCommand(this RouteGroupBuilder group)
    {
        group.MapPatch("/update", async (
            [FromBody] UpdateMemoryCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return ResultExtensions.HandleResult(result);
        })
        .RequireAuthorization(new PermissionAuthorizeAttribute(Modules.Memories, Permissions.Update));
    }
}
