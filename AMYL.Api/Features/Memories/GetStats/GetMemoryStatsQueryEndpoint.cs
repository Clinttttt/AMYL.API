using AMYL.Api.Shared.Abstractions;
using AMYL.Api.Shared.Security;
using AMYL.Api.Web.Authorization;
using AMYL.Api.Web.Extensions;
using MediatR;
using System.Security.Claims;

namespace AMYL.Api.Features.Memories.GetStats;

public static class GetMemoryStatsQueryEndpoint
{
    public static void MapGetMemoryStatsQuery(this RouteGroupBuilder group)
    {
        group.MapGet("/stats", async (
            ClaimsPrincipal claims,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var claimUserId = claims.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(claimUserId, out var userId))
            {
                return ResultExtensions.HandleResult(Result.Unauthorized("User id not found."));
            }

            var result = await sender.Send(new GetMemoryStatsQuery(userId), cancellationToken);
            return ResultExtensions.HandleResult(result);
        })
        .RequireAuthorization(new PermissionAuthorizeAttribute(Modules.Memories, Permissions.View));
    }
}
