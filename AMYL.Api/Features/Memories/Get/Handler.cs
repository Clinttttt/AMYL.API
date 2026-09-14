using AMYL.Api.Abstractions.Messaging;
using AMYL.Api.Domain.Common;
using AMYL.Api.Domain.Errors;
using AMYL.Api.Features.Memories.Shared;
using AMYL.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace AMYL.Api.Features.Memories.Get;

internal sealed class Handler(AppDbContext context, HybridCache cache)
    : IQueryHandler<Query, MemoryResponse>
{
    public async Task<Result<MemoryResponse>> Handle(
        Query request,
        CancellationToken cancellationToken)
    {
        var cacheKey = MemoryCacheKeys.Memory(request.UserId, request.MemoryId);

        var memory = await cache.GetOrCreateAsync<MemoryResponse?>(
            cacheKey,
            cancel => FindMemoryAsync(request.MemoryId, request.UserId, cancel),
            tags: [MemoryCacheKeys.MemoryTag(request.UserId)],
            cancellationToken: cancellationToken);

        return memory is null
            ? MemoryErrors.NotFound
            : Result<MemoryResponse>.Success(memory);
    }

    private async ValueTask<MemoryResponse?> FindMemoryAsync(
        Guid memoryId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await context.Memories.AsNoTracking()
            .Where(memory => memory.Id == memoryId && memory.UserId == userId)
            .Select(memory => new MemoryResponse(
                MemoryId: memory.Id,
                Title: memory.Title,
                Description: memory.Description,
                ImageUrl: memory.ImageUrl,
                VideoUrl: memory.VideoUrl,
                AudioUrl: memory.AudioUrl,
                CreatedAt: memory.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
