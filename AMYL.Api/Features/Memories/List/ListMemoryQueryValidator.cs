using FluentValidation;

namespace AMYL.Api.Features.Memories.List;

public sealed class ListMemoryQueryValidator : AbstractValidator<ListMemoryQuery>
{
    public ListMemoryQueryValidator()
    {
        RuleFor(query => query.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}
