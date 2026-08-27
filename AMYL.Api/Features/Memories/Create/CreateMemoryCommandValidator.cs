using AMYL.Api.Features.Memories.Create;
using FluentValidation;
using System.Linq.Expressions;

public sealed class CreateMemoryCommandValidator
    : AbstractValidator<CreateMemoryCommand>
{
    private const long MaximumFileSize = 10 * 1024 * 1024;

    public CreateMemoryCommandValidator()
    {
        AddMediaLocationRule(
            command => command.VideoUrl,
            mediaName: "Video");

        AddMediaLocationRule(
            command => command.ImageUrl,
            mediaName: "Image");

        AddMediaLocationRule(
            command => command.AudioUrl,
            mediaName: "Audio");
    }

    private void AddMediaLocationRule(
        Expression<Func<CreateMemoryCommand, IFormFile?>> property,
        string mediaName,
        bool isRequired = false)
    {
        var rule = RuleFor(property)
            .Cascade(CascadeMode.Stop);

        if (isRequired)
        {
            rule
                .NotEmpty()
                .WithMessage($"{mediaName} is required.");
        }
        rule
            .Must(file => file is null || file.Length <= MaximumFileSize)
            .WithMessage($"{mediaName} must not exceed 10 MB.");
    }

}
