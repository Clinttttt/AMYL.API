using AMYL.Api.Abstractions.Messaging;
using AMYL.Api.Domain.Common;
using AMYL.Api.Features.Memories.Shared;

namespace AMYL.Api.Features.Memories.List;

public sealed record Query(string? Search, int PageNumber = 1, int PageSize = 10)
    : IQuery<PaginatedList<MemoryResponse>>;
