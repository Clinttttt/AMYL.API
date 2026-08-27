using AMYL.Api.Shared.Abstractions;
using AMYL.Api.Shared.Caching;
using AMYL.Api.Features.Memories;
using AMYL.Api.Shared.Security;
using AMYL.Api.Infrastructure.Persistence;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using MemoryEntity = AMYL.Api.Shared.Domain.Entities.Memory;

namespace AMYL.Api.Features.Memories.Create
{
    public sealed class CreateMemoryCommandHandler(
        AppDbContext context,
        IFileService fileService,
        ICurrentUserService currentUserService,
        HybridCache cache,
        TimeProvider timeProvider) : IRequestHandler<CreateMemoryCommand, Result>
    {
        public async Task<Result> Handle(CreateMemoryCommand request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.UserId;
            if (!Guid.TryParse(userId, out var id))
                return Result.Unauthorized();

            var VideoUrl = request.VideoUrl != null
                ? await fileService.UploadFile(request.VideoUrl, cancellationToken) : null;
            var ImageUrl = request.ImageUrl != null
              ? await fileService.UploadFile(request.ImageUrl, cancellationToken) : null;
            var AudioUrl = request.AudioUrl != null
              ? await fileService.UploadFile(request.AudioUrl, cancellationToken) : null;

            var memory = MemoryEntity.Create(
                request.Title,
                request.Description,
                timeProvider.GetUtcNow().UtcDateTime,
                request.MemoryType,
                ImageUrl,
                VideoUrl,
                AudioUrl,
                request.SecretNote,
                userId: id
            );
            await context.Memories.AddAsync(memory, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            await cache.RemoveByTagAsync(CacheKeys.MemoryListsTag(id), cancellationToken);

            return Result.Success();
        }
    }
}
