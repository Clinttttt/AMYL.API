using AMYL.Api.Abstractions.Endpoints;
using AMYL.Api.Extensions;
using AMYL.Api.Features.Memories.Shared;
using AMYL.Api.Infrastructure.Authentication;
using MediatR;

namespace AMYL.Api.Features.Memories.ListRecent;

internal sealed class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/api/memories", async (
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new Query(), cancellationToken);

                return result.HandleResult();
            })
            .WithName("ListRecentMemories")
            .WithTags("Memory")
            .RequireAuthorization()
            .RequireAuthorization(new PermissionAuthorizeAttribute(
                Modules.Memories, Permissions.View))
            .Produces<List<MemoryResponse>>();
}
