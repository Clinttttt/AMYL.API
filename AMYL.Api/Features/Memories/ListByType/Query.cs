using AMYL.Api.Abstractions.Messaging;
using AMYL.Api.Domain;
using AMYL.Api.Domain.Common;
using AMYL.Api.Features.Memories.Shared;

namespace AMYL.Api.Features.Memories.ListByType;

public sealed record Query(
    MemoryType Type,
    string? Search,
    int PageSize = 10,
    int PageNumber = 1) : IQuery<PaginatedList<MemoryResponse>>;
