using AMYL.Api.Abstractions.Endpoints;
using AMYL.Api.Extensions;
using AMYL.Api.Infrastructure.Authentication;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AMYL.Api.Features.Memories.Update;

internal sealed class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPatch("/api/memories/update", async (
                [FromBody] Command command,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(command, cancellationToken);

                return result.HandleResult();
            })
            .WithName("UpdateMemory")
            .WithTags("Memory")
            .RequireAuthorization()
            .RequireAuthorization(new PermissionAuthorizeAttribute(
                Modules.Memories, Permissions.Update))
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);
}
