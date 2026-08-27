using AMYL.Api.Shared.Abstractions;
using AMYL.Api.Features.Memories;
using MediatR;

namespace AMYL.Api.Features.Memories.Get
{
    public sealed record GetMemoryQuery(Guid MemoryId, Guid UserId) : IRequest<Result<MemoryDto>>;

}
