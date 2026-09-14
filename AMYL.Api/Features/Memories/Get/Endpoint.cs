using System.Security.Claims;
using AMYL.Api.Abstractions.Endpoints;
using AMYL.Api.Domain.Common;
using AMYL.Api.Domain.Errors;
using AMYL.Api.Extensions;
using AMYL.Api.Features.Memories.Shared;
using AMYL.Api.Infrastructure.Authentication;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AMYL.Api.Features.Memories.Get;

internal sealed class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/api/memories/{memoryId:guid}", async (
                [FromRoute] Guid memoryId,
                ISender sender,
                ClaimsPrincipal claims,
                CancellationToken cancellationToken) =>
            {
                // Reading and parsing the claim is an HTTP concern, so it stays here
                // rather than on the query.
                var claimUserId = claims.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!Guid.TryParse(claimUserId, out var userId))
                {
                    return Result<MemoryResponse>.Failure(MemoryErrors.Unauthorized)
                        .HandleResult();
                }

                var result = await sender.Send(new Query(memoryId, userId), cancellationToken);

                return result.HandleResult();
            })
            .WithName("GetMemory")
            .WithTags("Memory")
            .RequireAuthorization()
            .RequireAuthorization(new PermissionAuthorizeAttribute(
                Modules.Memories, Permissions.View))
            .Produces<MemoryResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);
}
