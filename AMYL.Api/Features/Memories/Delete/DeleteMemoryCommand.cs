using AMYL.Api.Shared.Abstractions;
using MediatR;

namespace AMYL.Api.Features.Memories.Delete
{
    public sealed record DeleteMemoryCommand(Guid MemoryId) : IRequest<Result>;

}
