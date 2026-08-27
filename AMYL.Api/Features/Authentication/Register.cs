using AMYL.Api.Data;
using AMYL.Api.Domain;
using AMYL.Api.Domain.Common;
using AMYL.Api.Shared.Extensions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AMYL.Api.Features.Authentication;

public static class Register
{
    public sealed record Command(
        string UserName,
        string FullName,
        string Password,
        string Email) : IRequest<Result>;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.FullName)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(160);

            RuleFor(command => command.UserName)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(50);

            RuleFor(command => command.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(254);

            RuleFor(command => command.Password)
                .NotEmpty()
                .MinimumLength(10)
                .MaximumLength(128)
                .Matches("[a-z]")
                .WithMessage("Password must contain a lowercase letter.")
                .Matches("[A-Z]")
                .WithMessage("Password must contain an uppercase letter.")
                .Matches("[0-9]")
                .WithMessage("Password must contain a number.");
        }
    }

    internal sealed class Handler(
        AppDbContext context,
        IPasswordHasherService passwordHasher,
        TimeProvider timeProvider) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            if (await context.Users.AnyAsync(
                    user => user.UserName == request.UserName || user.Email == request.Email,
                    cancellationToken))
            {
                return Result.Conflict("Username already exists");
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

    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("/register", async (
            [FromBody] Command command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return ResultExtensions.HandleResult(result);
        });
}
