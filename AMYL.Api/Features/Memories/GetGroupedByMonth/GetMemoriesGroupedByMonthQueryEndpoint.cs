using AMYL.Api.Shared.Abstractions;
using AMYL.Api.Shared.Security;
using AMYL.Api.Web.Authorization;
using AMYL.Api.Web.Extensions;
using MediatR;
using System.Security.Claims;

namespace AMYL.Api.Features.Memories.GetGroupedByMonth;

public static class GetMemoriesGroupedByMonthQueryEndpoint
{
    public static void MapGetMemoriesGroupedByMonthQuery(this RouteGroupBuilder group)
    {
        group.MapGet("/group-by-month", async (
            ClaimsPrincipal user,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var claimUserId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(claimUserId, out var userId))
            {
                return ResultExtensions.HandleResult(Result.Unauthorized("User id not found."));
            }

            var result = await sender.Send(
                new GetMemoriesGroupedByMonthQuery(userId),
                cancellationToken);

            return ResultExtensions.HandleResult(result);
        })
        .RequireAuthorization(new PermissionAuthorizeAttribute(Modules.Memories, Permissions.View));
    }
}
