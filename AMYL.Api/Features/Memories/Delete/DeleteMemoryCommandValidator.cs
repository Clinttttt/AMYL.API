using FluentValidation;

namespace AMYL.Api.Features.Memories.Delete;

public sealed class DeleteMemoryCommandValidator : AbstractValidator<DeleteMemoryCommand>
{
    public DeleteMemoryCommandValidator()
    {
        RuleFor(command => command.MemoryId).NotEmpty();
    }
}
