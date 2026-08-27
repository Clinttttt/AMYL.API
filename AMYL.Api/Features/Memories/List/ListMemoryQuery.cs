using AMYL.Api.Shared.Abstractions;
using AMYL.Api.Features.Memories;
using MediatR;

namespace AMYL.Api.Features.Memories.List
{
    public sealed record ListMemoryQuery(string? search, int PageNumber = 1, int PageSize = 10) : IRequest<Result<PaginatedList<MemoryDto>>>
    {
    }


}
