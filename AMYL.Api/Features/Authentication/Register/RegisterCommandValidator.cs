using FluentValidation;

namespace AMYL.Api.Features.Authentication.Register
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
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
}
