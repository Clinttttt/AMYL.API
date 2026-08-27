using AMYL.Api.Shared.Abstractions;
using AMYL.Api.Shared.Caching;
using AMYL.Api.Shared.Security;
using AMYL.Api.Features.Memories;
using AMYL.Api.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
namespace AMYL.Api.Features.Memories.List
{
    public class ListMemoryQueryHandler(AppDbContext context, ICurrentUserService currentUserService, HybridCache cache) : IRequestHandler<ListMemoryQuery, Result<PaginatedList<MemoryDto>>>
    {
        public async Task<Result<PaginatedList<MemoryDto>>> Handle(ListMemoryQuery request, CancellationToken cancellationToken)
        {
            var Id = currentUserService.UserId;
            if (!Guid.TryParse(Id, out var id))
                return Result<PaginatedList<MemoryDto>>.Unauthorized();

            var cacheKey = CacheKeys.MemoryList(id, request.PageNumber, request.PageSize, request.search);

            var listing = await cache.GetOrCreateAsync<PaginatedList<MemoryDto>>(cacheKey, cancel =>
             GetListingAsync(id, request.search, request.PageNumber, request.PageSize, cancel),
             tags: [CacheKeys.MemoryListsTag(id)],
             cancellationToken: cancellationToken
            );
            return Result<PaginatedList<MemoryDto>>.Success(listing);
        }
        private async ValueTask<PaginatedList<MemoryDto>> GetListingAsync(Guid userId, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            var query = context.Memories
                .AsNoTracking()
                .Where(memory => memory.UserId == userId)
                .OrderByDescending(memory => memory.CreatedAt)
                .ThenBy(memory => memory.Id)
                .Select(memory => new MemoryDto(
                    MemoryId: memory.Id,
                    Title: memory.Title,
                    Description: memory.Description,
                    ImageUrl: memory.ImageUrl,
                    VideoUrl: memory.VideoUrl,
                    AudioUrl: memory.AudioUrl,
                    CreatedAt: memory.CreatedAt));

            query = query.WhereIf(
                !string.IsNullOrWhiteSpace(search),
                memory => memory.Title!.Contains(search!));

            return await query.ToPaginatedListAsync(
                pageNumber,
                pageSize,
                cancellationToken);
        }
    }
}
