using AMYL.Api.Shared.Abstractions;
using AMYL.Api.Features.Memories;
using MediatR;

namespace AMYL.Api.Features.Memories.GetRecent
{
    public sealed record GetRecentMemoriesQuery() : IRequest<Result<List<MemoryDto>>>;

}
