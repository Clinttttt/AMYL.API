using AMYL.Api.Abstractions.Messaging;

namespace AMYL.Api.Features.Memories.GetStats;

public sealed record Query(Guid UserId) : IQuery<Response>;
