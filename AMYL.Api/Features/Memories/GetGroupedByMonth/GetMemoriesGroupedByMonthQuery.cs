using AMYL.Api.Shared.Abstractions;
using AMYL.Api.Features.Memories;
using MediatR;

namespace AMYL.Api.Features.Memories.GetGroupedByMonth
{
    public sealed record GetMemoriesGroupedByMonthQuery(Guid UserId) : IRequest<Result<List<MemoryByMonthDto>>>;

}
