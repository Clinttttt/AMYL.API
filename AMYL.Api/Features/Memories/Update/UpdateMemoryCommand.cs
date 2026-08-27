using AMYL.Api.Shared.Abstractions;
using MediatR;
using static AMYL.Api.Shared.Domain.Entities.Enums;

namespace AMYL.Api.Features.Memories.Update
{
    public sealed record UpdateMemoryCommand(
        Guid MemoryId,
        string Title,
        string Description,
        MemoryType MemoryType,
        IFormFile? VideoUrl,
        IFormFile? ImageUrl,
        IFormFile? AudioUrl
        ) : IRequest<Result>
    {
    }
}
