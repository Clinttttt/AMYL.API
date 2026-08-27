using AMYL.Api.Shared.Security;
using AMYL.Api.Web.Authorization;
using AMYL.Api.Web.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static AMYL.Api.Shared.Domain.Entities.Enums;

namespace AMYL.Api.Features.Memories.GetByType;

public static class GetMemoriesByTypeQueryEndpoint
{
    public static void MapGetMemoriesByTypeQuery(this RouteGroupBuilder group)
    {
        group.MapGet("/type", async (
            [FromQuery] MemoryType type,
            [FromQuery] string? search,
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetMemoriesByTypeQuery(type, search, pageSize, pageNumber);
            var result = await sender.Send(query, cancellationToken);
            return ResultExtensions.HandleResult(result);
        })
        .RequireAuthorization(new PermissionAuthorizeAttribute(Modules.Memories, Permissions.View));
    }
}
