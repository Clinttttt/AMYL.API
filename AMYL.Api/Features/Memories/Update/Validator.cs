using FluentValidation;

namespace AMYL.Api.Features.Memories.Update;

internal sealed class Validator : AbstractValidator<Command>
{
    private const long MaximumFileSize = 10 * 1024 * 1024;

    public Validator()
    {
        RuleFor(command => command.MemoryId).NotEmpty();
        RuleFor(command => command.Title).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Description).MaximumLength(4_000);

        RuleFor(command => command.VideoUrl)
            .Must(file => file is null || file.Length <= MaximumFileSize)
            .WithMessage("Video must not exceed 10 MB.");

        RuleFor(command => command.ImageUrl)
            .Must(file => file is null || file.Length <= MaximumFileSize)
            .WithMessage("Image must not exceed 10 MB.");

        RuleFor(command => command.AudioUrl)
            .Must(file => file is null || file.Length <= MaximumFileSize)
            .WithMessage("Audio must not exceed 10 MB.");
    }
}
