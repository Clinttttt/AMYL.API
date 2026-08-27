using AMYL.Api.Shared.Abstractions;
using AMYL.Api.Features.Memories;
using MediatR;
using static AMYL.Api.Shared.Domain.Entities.Enums;

namespace AMYL.Api.Features.Memories.GetByType
{
    public sealed record GetMemoriesByTypeQuery(MemoryType Type, string? Search, int PageSize = 10, int PageNumber = 1) : IRequest<Result<PaginatedList<MemoryDto>>>;

}
