using FluentValidation;

namespace AMYL.Api.Features.Memories.GetByType;

public sealed class GetMemoriesByTypeQueryValidator : AbstractValidator<GetMemoriesByTypeQuery>
{
    public GetMemoriesByTypeQueryValidator()
    {
        RuleFor(query => query.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}
