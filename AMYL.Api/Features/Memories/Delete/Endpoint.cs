using AMYL.Api.Abstractions.Endpoints;
using AMYL.Api.Extensions;
using AMYL.Api.Infrastructure.Authentication;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AMYL.Api.Features.Memories.Delete;

internal sealed class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapDelete("/api/memories/delete", async (
                [FromBody] Command command,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(command, cancellationToken);

                return result.HandleResult();
            })
            .WithName("DeleteMemory")
            .WithTags("Memory")
            .RequireAuthorization()
            .RequireAuthorization(new PermissionAuthorizeAttribute(
                Modules.Memories, Permissions.Delete))
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);
}
