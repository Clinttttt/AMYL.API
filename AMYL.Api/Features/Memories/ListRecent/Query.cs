using AMYL.Api.Abstractions.Messaging;
using AMYL.Api.Features.Memories.Shared;

namespace AMYL.Api.Features.Memories.ListRecent;

public sealed record Query : IQuery<List<MemoryResponse>>;
