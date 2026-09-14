using AMYL.Api.Abstractions.Messaging;
using AMYL.Api.Domain.Common;
using AMYL.Api.Domain.Errors;
using AMYL.Api.Features.Memories.Shared;
using AMYL.Api.Infrastructure.Authentication;
using AMYL.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using MemoryEntity = AMYL.Api.Domain.Memory;

namespace AMYL.Api.Features.Memories.GroupByMonth;

internal sealed class Handler(
    AppDbContext context,
    HybridCache cache,
    ICurrentUserService currentUserService) : IQueryHandler<Query, List<Response>>
{
    public async Task<Result<List<Response>>> Handle(
        Query request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(currentUserService.UserId, out var userId))
        {
            return MemoryErrors.Unauthorized;
        }

        var query = context.Memories
            .Where(memory => memory.UserId == userId)
            .AsNoTracking()
            .OrderByDescending(memory => memory.CreatedAt);

        var cacheKey = MemoryCacheKeys.MemoriesGroupByMonth(userId);

        var grouped = await cache.GetOrCreateAsync(
            cacheKey,
            async cancel => await GetListing(query, cancel),
            tags: [MemoryCacheKeys.MemoryListsTag(userId)],
            cancellationToken: cancellationToken);

        return Result<List<Response>>.Success(grouped);
    }

    private static async Task<List<Response>> GetListing(
        IQueryable<MemoryEntity> query,
        CancellationToken cancellationToken)
    {
        return await query
            .Select(memory => new MemoryResponse(
                MemoryId: memory.Id,
                Title: memory.Title,
                Description: memory.Description,
                ImageUrl: memory.ImageUrl,
                VideoUrl: memory.VideoUrl,
                AudioUrl: memory.AudioUrl,
                CreatedAt: memory.CreatedAt))
            .GroupBy(memory => new
            {
                year = memory.CreatedAt.Year,
                month = memory.CreatedAt.Month
            })
            .Select(group => new Response(
                MonthLabel: new DateTime(group.Key.year, group.Key.month, 1)
                    .ToString("MMMM, yyyy"),
                Memories: group.ToList()))
            .ToListAsync(cancellationToken);
    }
}
