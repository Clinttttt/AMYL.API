using FluentValidation;

namespace AMYL.Api.Features.Authentication.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.UserName).NotEmpty();
        RuleFor(command => command.Password).NotEmpty();
    }
}
