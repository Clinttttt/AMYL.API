using AMYL.Api.Abstractions.Messaging;

namespace AMYL.Api.Features.Memories.GroupByMonth;

public sealed record Query(Guid UserId) : IQuery<List<Response>>;
