using AMYL.Api.Shared.Abstractions;
using MediatR;

namespace AMYL.Api.Features.Memories.GetStats
{
    public sealed record GetMemoryStatsQuery(Guid UserId) : IRequest<Result<MemoryStatsDto>>;

}
