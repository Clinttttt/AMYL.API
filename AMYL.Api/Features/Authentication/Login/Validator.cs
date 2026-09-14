using FluentValidation;

namespace AMYL.Api.Features.Authentication.Login;

internal sealed class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(command => command.UserName).NotEmpty();
        RuleFor(command => command.Password).NotEmpty();
    }
}
