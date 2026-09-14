using AMYL.Api.Abstractions.Messaging;
using AMYL.Api.Domain.Common;
using AMYL.Api.Domain.Errors;
using AMYL.Api.Features.Memories.Shared;
using AMYL.Api.Infrastructure.Authentication;
using AMYL.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace AMYL.Api.Features.Memories.ListRecent;

internal sealed class Handler(
    AppDbContext context,
    HybridCache cache,
    ICurrentUserService currentUserService)
    : IQueryHandler<Query, List<MemoryResponse>>
{
    private const int RecentCount = 10;

    public async Task<Result<List<MemoryResponse>>> Handle(
        Query request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(currentUserService.UserId, out var userId))
        {
            return MemoryErrors.Unauthorized;
        }

        var cacheKey = MemoryCacheKeys.RecentMemories(userId);

        var memories = await cache.GetOrCreateAsync<List<MemoryResponse>>(
            cacheKey,
            async cancel => await context.Memories
                .AsNoTracking()
                .Where(memory => memory.UserId == userId)
                .OrderByDescending(memory => memory.CreatedAt)
                .Select(memory => new MemoryResponse(
                    MemoryId: memory.Id,
                    Title: memory.Title,
                    Description: memory.Description,
                    ImageUrl: memory.ImageUrl,
                    VideoUrl: memory.VideoUrl,
                    AudioUrl: memory.AudioUrl,
                    CreatedAt: memory.CreatedAt))
                .Take(RecentCount)
                .ToListAsync(cancel),
            tags: [MemoryCacheKeys.MemoryListsTag(userId)],
            cancellationToken: cancellationToken);

        return Result<List<MemoryResponse>>.Success(memories);
    }
}
