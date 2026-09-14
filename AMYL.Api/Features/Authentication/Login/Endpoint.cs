using AMYL.Api.Abstractions.Endpoints;
using AMYL.Api.Extensions;
using AMYL.Api.Infrastructure.Authentication;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AMYL.Api.Features.Authentication.Login;

internal sealed class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/api/auth/login", async (
                [FromBody] Command command,
                ISender sender,
                HttpResponse response,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(command, cancellationToken);

                if (result.IsSuccess)
                {
                    // Writing the auth cookies is an HTTP concern and stays at the endpoint.
                    AuthCookies.SetAuthCookies(
                        response,
                        result.Value!.AccessToken,
                        result.Value.RefreshToken);
                }

                return result.HandleResult();
            })
            .WithName("Login")
            .WithTags("Auth")
            .Produces<TokenResponse>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized);
}
