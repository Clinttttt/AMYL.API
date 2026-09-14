using AMYL.Api.Abstractions.Messaging;
using AMYL.Api.Domain.Common;
using AMYL.Api.Domain.Errors;
using AMYL.Api.Features.Memories.Shared;
using AMYL.Api.Infrastructure.Authentication;
using AMYL.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using MemoryEntity = AMYL.Api.Domain.Memory;

namespace AMYL.Api.Features.Memories.Update;

internal sealed class Handler(
    AppDbContext context,
    IFileStorage fileStorage,
    HybridCache cache,
    ICurrentUserService currentUserService) : ICommandHandler<Command>
{
    public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(currentUserService.UserId, out var userId))
        {
            return MemoryErrors.Unauthorized;
        }

        var memory = await context.Memories
            .FirstOrDefaultAsync(
                memory => memory.Id == request.MemoryId && memory.UserId == userId,
                cancellationToken);

        if (memory is null)
        {
            return MemoryErrors.NotFound;
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

        MemoryEntity.Update(
            memory,
            request.Title,
            request.Description,
            request.MemoryType,
            imageUrl,
            videoUrl,
            audioUrl);

        await context.SaveChangesAsync(cancellationToken);

        await cache.RemoveAsync(
            MemoryCacheKeys.Memory(userId, request.MemoryId), cancellationToken);
        await cache.RemoveByTagAsync(
            MemoryCacheKeys.MemoryListsTag(userId), cancellationToken);

        return Result.Success();
    }
}
