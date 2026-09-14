using System.Linq.Expressions;
using FluentValidation;

namespace AMYL.Api.Features.Memories.Create;

internal sealed class Validator : AbstractValidator<Command>
{
    private const long MaximumFileSize = 10 * 1024 * 1024;

    public Validator()
    {
        AddMediaRule(command => command.VideoUrl, "Video");
        AddMediaRule(command => command.ImageUrl, "Image");
        AddMediaRule(command => command.AudioUrl, "Audio");
    }

    private void AddMediaRule(
        Expression<Func<Command, IFormFile?>> property,
        string mediaName,
        bool isRequired = false)
    {
        var rule = RuleFor(property).Cascade(CascadeMode.Stop);

        if (isRequired)
        {
            rule.NotEmpty().WithMessage($"{mediaName} is required.");
        }

        rule
            .Must(file => file is null || file.Length <= MaximumFileSize)
            .WithMessage($"{mediaName} must not exceed 10 MB.");
    }
}
