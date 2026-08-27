using AMYL.Api.Shared.Security;
using AMYL.Api.Web.Authorization;
using AMYL.Api.Web.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AMYL.Api.Features.Memories.List;

public static class ListMemoryQueryEndpoint
{
    public static void MapListMemoryQuery(this RouteGroupBuilder group)
    {
        group.MapGet("/list", async (
            ISender sender,
            CancellationToken cancellationToken,
            [FromQuery] string? search,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10) =>
        {
            var query = new ListMemoryQuery(search, pageNumber, pageSize);
            var result = await sender.Send(query, cancellationToken);
            return ResultExtensions.HandleResult(result);
        })
        .RequireAuthorization(new PermissionAuthorizeAttribute(Modules.Memories, Permissions.View));
    }
}
