using AMYL.Api.Shared.Abstractions;
using AMYL.Api.Features.Authentication;
using AMYL.Api.Infrastructure.Persistence;
using AMYL.Api.Features.Authentication.Login;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

namespace AMYL.Api.Features.Authentication.Login
{
    public class LoginCommandHandler(IPasswordHasherService passwordHasher, AppDbContext context, ITokenService tokenService) : IRequestHandler<LoginCommand, Result<TokenResponseDto>>
    {
        public async Task<Result<TokenResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await context.Users.FirstOrDefaultAsync(
                user => user.UserName == request.UserName,
                cancellationToken);

            if (user is null)
                return Result<TokenResponseDto>.NotFound("User not found");

            if (!passwordHasher.VerifyPassword(user.PasswordHash, request.Password))
            {
                return Result<TokenResponseDto>.Unauthorized();
            }

            var token = await tokenService.CreateTokenResponseAsync(user, cancellationToken);
            return Result<TokenResponseDto>.Success(token);
        }
    }
}
