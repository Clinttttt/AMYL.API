using AMYL.Api.Data;
using AMYL.Api.Domain.Common;
using AMYL.Api.Shared.Extensions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AMYL.Api.Features.Authentication;

public static class Login
{
    public sealed record Command(string UserName, string Password)
        : IRequest<Result<TokenResponse>>;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.UserName).NotEmpty();
            RuleFor(command => command.Password).NotEmpty();
        }
    }

    internal sealed class Handler(
        IPasswordHasherService passwordHasher,
        AppDbContext context,
        ITokenService tokenService) : IRequestHandler<Command, Result<TokenResponse>>
    {
        public async Task<Result<TokenResponse>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var user = await context.Users.FirstOrDefaultAsync(
                user => user.UserName == request.UserName,
                cancellationToken);

            if (user is null)
            {
                return Result<TokenResponse>.NotFound("User not found");
            }

            if (!passwordHasher.VerifyPassword(user.PasswordHash, request.Password))
            {
                return Result<TokenResponse>.Unauthorized();
            }

            var token = await tokenService.CreateTokenResponseAsync(user, cancellationToken);
            return Result<TokenResponse>.Success(token);
        }
    }

    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("/login", async (
            [FromBody] Command command,
            ISender sender,
            HttpResponse response,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                AuthCookies.SetAuthCookies(
                    response,
                    result.Value!.AccessToken,
                    result.Value.RefreshToken);
            }

            return ResultExtensions.HandleResult(result);
        });
}
