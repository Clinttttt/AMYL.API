using AMYL.Api.Abstractions.Endpoints;
using AMYL.Api.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AMYL.Api.Features.Authentication.Register;

internal sealed class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/api/auth/register", async (
                [FromBody] Command command,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(command, cancellationToken);

                return result.HandleResult();
            })
            .WithName("Register")
            .WithTags("Auth")
            .Produces(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);
}
