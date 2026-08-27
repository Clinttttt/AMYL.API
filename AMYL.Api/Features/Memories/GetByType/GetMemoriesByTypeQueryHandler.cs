using AMYL.Api.Shared.Abstractions;
using AMYL.Api.Shared.Caching;
using AMYL.Api.Shared.Security;
using AMYL.Api.Features.Memories;
using AMYL.Api.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using static AMYL.Api.Shared.Domain.Entities.Enums;

namespace AMYL.Api.Features.Memories.GetByType
{
    public class GetMemoriesByTypeQueryHandler(AppDbContext context, ICurrentUserService currentUserService, HybridCache cache) : IRequestHandler<GetMemoriesByTypeQuery, Result<PaginatedList<MemoryDto>>>
    {
        public async Task<Result<PaginatedList<MemoryDto>>> Handle(GetMemoriesByTypeQuery request, CancellationToken cancellationToken)
        {
            var id = currentUserService.UserId;
            if (!Guid.TryParse(id, out var userId))
                return Result<PaginatedList<MemoryDto>>.Unauthorized();

            var cacheKey = CacheKeys.MemoriesByType(
                userId,
                request.Type,
                request.PageNumber,
                request.PageSize,
                request.Search);

            var Listing = await cache.GetOrCreateAsync<PaginatedList<MemoryDto>>(cacheKey, cancel =>
              PaginatedListAsync(userId, request.Type, request.Search, request.PageNumber, request.PageSize, cancel),
              tags: [CacheKeys.MemoryListsTag(userId)],
              cancellationToken: cancellationToken);
            return Result<PaginatedList<MemoryDto>>.Success(Listing);
        }
        private async ValueTask<PaginatedList<MemoryDto>> PaginatedListAsync(Guid userId, MemoryType type, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = context.Memories
                .AsNoTracking()
                .Where(s => s.UserId == userId && s.MemoryType == type)
                .OrderByDescending(s => s.CreatedAt)
                .ThenBy(s => s.Id)
                .Select(m => new MemoryDto(
                     MemoryId: m.Id,
                     Title: m.Title,
                     Description: m.Description,
                     ImageUrl: m.ImageUrl,
                     VideoUrl: m.VideoUrl,
                     AudioUrl: m.AudioUrl,
                     CreatedAt: m.CreatedAt
                    ));

            var searchTrim = search?.Trim();
            query = query.WhereIf(
                 !string.IsNullOrWhiteSpace(searchTrim),
                  s => s.Title!.Contains(searchTrim!)
                );
            return await QueryableExtensions.ToPaginatedListAsync(query, pageNumber, pageSize, cancellationToken);
        }
    }
}
