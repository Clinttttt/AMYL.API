using FluentValidation;

namespace AMYL.Api.Features.Memories.ListByType;

internal sealed class Validator : AbstractValidator<Query>
{
    public Validator()
    {
        RuleFor(query => query.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}
