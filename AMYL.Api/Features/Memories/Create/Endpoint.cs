using AMYL.Api.Abstractions.Endpoints;
using AMYL.Api.Extensions;
using AMYL.Api.Infrastructure.Authentication;
using AMYL.Api.Infrastructure.RateLimiting;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AMYL.Api.Features.Memories.Create;

internal sealed class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/api/memories/create", async (
                [FromBody] Command command,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(command, cancellationToken);

                return result.HandleResult();
            })
            .WithName("CreateMemory")
            .WithTags("Memory")
            .RequireRateLimiting(RateLimitPolicies.General)
            .RequireAuthorization()
            .RequireAuthorization(new PermissionAuthorizeAttribute(
                Modules.Memories, Permissions.Create))
            .ProducesValidationProblem();
}
