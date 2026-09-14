using FluentValidation;

namespace AMYL.Api.Features.Memories.Delete;

internal sealed class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(command => command.MemoryId).NotEmpty();
    }
}
