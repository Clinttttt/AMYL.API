using AMYL.Api.Abstractions.Messaging;
using AMYL.Api.Domain;
using AMYL.Api.Domain.Common;
using AMYL.Api.Domain.Errors;
using AMYL.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AMYL.Api.Features.Authentication.Register;

internal sealed class Handler(
    AppDbContext context,
    IPasswordHasherService passwordHasher,
    TimeProvider timeProvider) : ICommandHandler<Command>
{
    public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
    {
        if (await context.Users.AnyAsync(
                user => user.UserName == request.UserName || user.Email == request.Email,
                cancellationToken))
        {
            return UserErrors.DuplicateUserName;
        }

        var passwordHash = passwordHasher.HashPassword(request.Password);

        var user = Users.Create(
            request.UserName,
            request.FullName,
            passwordHash,
            request.Email,
            timeProvider.GetUtcNow().UtcDateTime);

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
