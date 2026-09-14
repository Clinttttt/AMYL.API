using AMYL.Api.Abstractions.Messaging;
using AMYL.Api.Domain.Common;
using AMYL.Api.Domain.Errors;
using AMYL.Api.Features.Memories.Shared;
using AMYL.Api.Infrastructure.Authentication;
using AMYL.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace AMYL.Api.Features.Memories.List;

internal sealed class Handler(
    AppDbContext context,
    ICurrentUserService currentUserService,
    HybridCache cache) : IQueryHandler<Query, PaginatedList<MemoryResponse>>
{
    public async Task<Result<PaginatedList<MemoryResponse>>> Handle(
        Query request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(currentUserService.UserId, out var userId))
        {
            return MemoryErrors.Unauthorized;
        }

        var cacheKey = MemoryCacheKeys.MemoryList(
            userId, request.PageNumber, request.PageSize, request.Search);

        var listing = await cache.GetOrCreateAsync<PaginatedList<MemoryResponse>>(
            cacheKey,
            cancel => GetListingAsync(
                userId, request.Search, request.PageNumber, request.PageSize, cancel),
            tags: [MemoryCacheKeys.MemoryListsTag(userId)],
            cancellationToken: cancellationToken);

        return Result<PaginatedList<MemoryResponse>>.Success(listing);
    }

    private async ValueTask<PaginatedList<MemoryResponse>> GetListingAsync(
        Guid userId,
        string? search,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = context.Memories
            .AsNoTracking()
            .Where(memory => memory.UserId == userId)
            .OrderByDescending(memory => memory.CreatedAt)
            .ThenBy(memory => memory.Id)
            .Select(memory => new MemoryResponse(
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

        return await query.ToPaginatedListAsync(pageNumber, pageSize, cancellationToken);
    }
}
