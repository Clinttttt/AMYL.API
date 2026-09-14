using AMYL.Api.Abstractions.Messaging;
using AMYL.Api.Features.Memories.Shared;

namespace AMYL.Api.Features.Memories.Get;

public sealed record Query(Guid MemoryId, Guid UserId) : IQuery<MemoryResponse>;
