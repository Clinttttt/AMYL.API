using AMYL.Api.Abstractions.Endpoints;
using AMYL.Api.Domain.Common;
using AMYL.Api.Extensions;
using AMYL.Api.Features.Memories.Shared;
using AMYL.Api.Infrastructure.Authentication;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AMYL.Api.Features.Memories.List;

internal sealed class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/api/memories/list", async (
                ISender sender,
                CancellationToken cancellationToken,
                [FromQuery] string? search,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10) =>
            {
                var result = await sender.Send(
                    new Query(search, pageNumber, pageSize), cancellationToken);

                return result.HandleResult();
            })
            .WithName("ListMemories")
            .WithTags("Memory")
            .RequireAuthorization()
            .RequireAuthorization(new PermissionAuthorizeAttribute(
                Modules.Memories, Permissions.View))
            .Produces<PaginatedList<MemoryResponse>>()
            .ProducesValidationProblem();
}
