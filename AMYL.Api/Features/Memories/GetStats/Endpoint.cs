using System.Security.Claims;
using AMYL.Api.Abstractions.Endpoints;
using AMYL.Api.Domain.Common;
using AMYL.Api.Domain.Errors;
using AMYL.Api.Extensions;
using AMYL.Api.Infrastructure.Authentication;
using MediatR;

namespace AMYL.Api.Features.Memories.GetStats;

internal sealed class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/api/memories/stats", async (
                ClaimsPrincipal claims,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var claimUserId = claims.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!Guid.TryParse(claimUserId, out var userId))
                {
                    return Result.Failure(MemoryErrors.Unauthorized).HandleResult();
                }

                var result = await sender.Send(new Query(userId), cancellationToken);

                return result.HandleResult();
            })
            .WithName("GetMemoryStats")
            .WithTags("Memory")
            .RequireAuthorization()
            .RequireAuthorization(new PermissionAuthorizeAttribute(
                Modules.Memories, Permissions.View))
            .Produces<Response>();
}
