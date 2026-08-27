using AMYL.Api.Shared.Abstractions;
using AMYL.Api.Features.Authentication;
using AMYL.Api.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AMYL.Api.Features.Authentication.Register
{
    public sealed class RegisterCommandHandler(
        AppDbContext context,
        IPasswordHasherService passwordHasher,
        TimeProvider timeProvider) : IRequestHandler<RegisterCommand, Result>
    {
        public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            if (await context.Users.AnyAsync(
                user => user.UserName == request.UserName || user.Email == request.Email,
                cancellationToken))
            {
                return Result.Conflict("Username already exists");
            }

            var passwordHash = passwordHasher.HashPassword(request.Password);
            var create = AMYL.Api.Shared.Domain.Entities.Users.Create(
                request.UserName,
                request.FullName,
                passwordHash,
                request.Email,
                timeProvider.GetUtcNow().UtcDateTime);
            context.Users.Add(create);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
