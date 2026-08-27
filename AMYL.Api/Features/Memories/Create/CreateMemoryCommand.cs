using AMYL.Api.Shared.Abstractions;
using MediatR;
using static AMYL.Api.Shared.Domain.Entities.Enums;

namespace AMYL.Api.Features.Memories.Create
{
    public sealed record CreateMemoryCommand(
        string Title,
        string Description,
        MemoryType MemoryType,
        IFormFile VideoUrl,
        IFormFile? ImageUrl,
        IFormFile? AudioUrl,
        string? SecretNote
        ) : IRequest<Result>;

}
