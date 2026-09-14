using AMYL.Api.Abstractions.Endpoints;
using AMYL.Api.Domain;
using AMYL.Api.Domain.Common;
using AMYL.Api.Extensions;
using AMYL.Api.Features.Memories.Shared;
using AMYL.Api.Infrastructure.Authentication;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AMYL.Api.Features.Memories.ListByType;

internal sealed class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/api/memories/type", async (
                [FromQuery] MemoryType type,
                [FromQuery] string? search,
                [FromQuery] int pageSize,
                [FromQuery] int pageNumber,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new Query(type, search, pageSize, pageNumber), cancellationToken);

                return result.HandleResult();
            })
            .WithName("ListMemoriesByType")
            .WithTags("Memory")
            .RequireAuthorization()
            .RequireAuthorization(new PermissionAuthorizeAttribute(
                Modules.Memories, Permissions.View))
            .Produces<PaginatedList<MemoryResponse>>()
            .ProducesValidationProblem();
}
