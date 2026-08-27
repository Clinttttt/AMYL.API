using AMYL.Api.Shared.Abstractions;
using AMYL.Api.Shared.Security;
using AMYL.Api.Web.Authorization;
using AMYL.Api.Web.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AMYL.Api.Features.Memories.Get;

public static class GetMemoryQueryEndpoint
{
    public static void MapMemoryQuery(this RouteGroupBuilder group)
    {
        group.MapGet("/{memoryId:guid}", async (
            [FromRoute] Guid memoryId,
            ISender sender,
            ClaimsPrincipal claims,
            CancellationToken cancellationToken) =>
        {
            var claimUserId = claims.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(claimUserId, out var userId))
            {
                return ResultExtensions.HandleResult(Result<MemoryDto>.Unauthorized());
            }

            var result = await sender.Send(
                new GetMemoryQuery(memoryId, userId),
                cancellationToken);

            return ResultExtensions.HandleResult(result);
        })
        .RequireAuthorization(new PermissionAuthorizeAttribute(Modules.Memories, Permissions.View));
    }
}
