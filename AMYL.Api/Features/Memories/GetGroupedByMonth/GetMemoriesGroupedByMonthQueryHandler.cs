using AMYL.Api.Features.Memories;
using AMYL.Api.Infrastructure.Persistence;
using AMYL.Api.Shared.Abstractions;
using AMYL.Api.Shared.Caching;
using AMYL.Api.Shared.Domain.Entities;
using AMYL.Api.Shared.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.IdentityModel.Tokens.Experimental;
using System.Threading.Channels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace AMYL.Api.Features.Memories.GetGroupedByMonth
{
    public class GetMemoriesGroupedByMonthQueryHandler(AppDbContext context, HybridCache cache, ICurrentUserService currentUserService) : IRequestHandler<GetMemoriesGroupedByMonthQuery, Result<List<MemoryByMonthDto>>>
    {
        public async Task<Result<List<MemoryByMonthDto>>> Handle(GetMemoriesGroupedByMonthQuery request, CancellationToken cancellationToken)
        {
            var Id = currentUserService.UserId;
            if (!Guid.TryParse(Id, out var userId))
                return Result<List<MemoryByMonthDto>>.Unauthorized();

            var query = context.Memories.Where(s=> s.UserId == userId)
                .AsNoTracking()
                .OrderByDescending(s => s.CreatedAt);

            var cacheKey = CacheKeys.MemoriesGroupByMonth(userId);

            var result = await cache.GetOrCreateAsync(cacheKey, async (cancel) =>
                await GetListing(query, cancel),
                tags: [CacheKeys.MemoryListsTag(userId)],
                cancellationToken: cancellationToken
            );
          
            return Result<List<MemoryByMonthDto>>.Success(result);

        }
        public async Task<List<MemoryByMonthDto>> GetListing(IQueryable<Memory> query, CancellationToken cancellationToken)
        {
            return await query
                .Select(f => new MemoryDto(
                    MemoryId: f.Id,
                    Title: f.Title,
                    Description: f.Description,
                    ImageUrl: f.ImageUrl,
                    VideoUrl: f.VideoUrl,
                    AudioUrl: f.AudioUrl,
                    CreatedAt: f.CreatedAt))
                .GroupBy(g => new { year = g.CreatedAt.Year, month = g.CreatedAt.Month })
                .Select(s => new MemoryByMonthDto(
                    MonthLabel: new DateTime(s.Key.year, s.Key.month, 1).ToString("MMMM, yyyy"),
                    memories: s.ToList()))
                .ToListAsync(cancellationToken);
        }
    }
}
