using AMYL.Api.Abstractions.Messaging;
using AMYL.Api.Domain.Common;
using AMYL.Api.Domain.Errors;
using AMYL.Api.Features.Memories.Shared;
using AMYL.Api.Infrastructure.Authentication;
using AMYL.Api.Infrastructure.Persistence;
using Microsoft.Extensions.Caching.Hybrid;
using MemoryEntity = AMYL.Api.Domain.Memory;

namespace AMYL.Api.Features.Memories.Create;

internal sealed class Handler(
    AppDbContext context,
    IFileStorage fileStorage,
    ICurrentUserService currentUserService,
    HybridCache cache,
    TimeProvider timeProvider) : ICommandHandler<Command>
{
    public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(currentUserService.UserId, out var userId))
        {
            return MemoryErrors.Unauthorized;
        }

        var videoUrl = request.VideoUrl != null
            ? await fileStorage.UploadFile(request.VideoUrl, cancellationToken)
            : null;
        var imageUrl = request.ImageUrl != null
            ? await fileStorage.UploadFile(request.ImageUrl, cancellationToken)
            : null;
        var audioUrl = request.AudioUrl != null
            ? await fileStorage.UploadFile(request.AudioUrl, cancellationToken)
            : null;

        var memory = MemoryEntity.Create(
            request.Title,
            request.Description,
            timeProvider.GetUtcNow().UtcDateTime,
            request.MemoryType,
            imageUrl,
            videoUrl,
            audioUrl,
            request.SecretNote,
            userId: userId);

        await context.Memories.AddAsync(memory, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await cache.RemoveByTagAsync(MemoryCacheKeys.MemoryListsTag(userId), cancellationToken);

        return Result.Success();
    }
}
