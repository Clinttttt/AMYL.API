using AMYL.Api.Abstractions.Messaging;
using AMYL.Api.Domain.Common;
using AMYL.Api.Domain.Errors;
using AMYL.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AMYL.Api.Features.Authentication.Login;

internal sealed class Handler(
    IPasswordHasherService passwordHasher,
    AppDbContext context,
    ITokenService tokenService) : ICommandHandler<Command, TokenResponse>
{
    public async Task<Result<TokenResponse>> Handle(
        Command request,
        CancellationToken cancellationToken)
    {
        var user = await context.Users.FirstOrDefaultAsync(
            user => user.UserName == request.UserName,
            cancellationToken);

        // Same error for an unknown user and a wrong password: distinguishing them
        // tells an attacker which usernames exist.
        if (user is null || !passwordHasher.VerifyPassword(user.PasswordHash, request.Password))
        {
            return UserErrors.InvalidCredentials;
        }

        var token = await tokenService.CreateTokenResponseAsync(user, cancellationToken);

        return Result<TokenResponse>.Success(token);
    }
}
