using AMYL.Api.Features.Memories;
using AMYL.Api.Infrastructure.Persistence;
using AMYL.Api.Shared.Abstractions;
using AMYL.Api.Shared.Caching;
using AMYL.Api.Shared.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using System.Threading.Channels;

namespace AMYL.Api.Features.Memories.GetRecent
{
    public class GetRecentMemoriesQueryHandler(AppDbContext context, HybridCache cache, ICurrentUserService currentUserService) : IRequestHandler<GetRecentMemoriesQuery, Result<List<MemoryDto>>>
    {
        public async Task<Result<List<MemoryDto>>> Handle(GetRecentMemoriesQuery request, CancellationToken cancellationToken)
        {
            var Id = currentUserService.UserId;
            if (!Guid.TryParse(Id, out var UserId))
                return Result<List<MemoryDto>>.Unauthorized();

            var cacheKey = CacheKeys.RecentMemories(UserId);

            var result = await cache.GetOrCreateAsync<List<MemoryDto>>(cacheKey, async (cancel) =>
             await context.Memories
              .AsNoTracking()
              .OrderByDescending(s => s.CreatedAt)
              .Where(s=> s.UserId == UserId)
              .Select(f => new MemoryDto(
                  MemoryId: f.Id,
                  Title: f.Title,
                  Description: f.Description,
                  ImageUrl: f.ImageUrl,
                  VideoUrl: f.VideoUrl,
                  AudioUrl: f.AudioUrl,
                  CreatedAt: f.CreatedAt
                  )).Take(10)
                  .ToListAsync(cancel),
                  tags: [CacheKeys.MemoryListsTag(UserId)],
                  cancellationToken: cancellationToken
            );
           
            return Result<List<MemoryDto>>.Success(result);

        }
    }
}
