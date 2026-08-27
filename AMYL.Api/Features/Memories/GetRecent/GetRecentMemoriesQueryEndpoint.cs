using AMYL.Api.Shared.Security;
using AMYL.Api.Web.Authorization;
using AMYL.Api.Web.Extensions;
using MediatR;

namespace AMYL.Api.Features.Memories.GetRecent;

public static class GetRecentMemoriesQueryEndpoint
{
    public static void MapGetRecentMemoriesQuery(this RouteGroupBuilder group)
    {
        group.MapGet("", async (ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetRecentMemoriesQuery(), cancellationToken);
            return ResultExtensions.HandleResult(result);
        })
        .RequireAuthorization(new PermissionAuthorizeAttribute(Modules.Memories, Permissions.View));
    }
}
