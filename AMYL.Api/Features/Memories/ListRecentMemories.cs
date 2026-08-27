using AMYL.Api.Data;
using AMYL.Api.Domain.Common;
using AMYL.Api.Features.Memories.Shared;
using AMYL.Api.Shared.Authorization;
using AMYL.Api.Shared.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace AMYL.Api.Features.Memories;

public static class ListRecentMemories
{
    public sealed record Query() : IRequest<Result<List<MemoryResponse>>>;

    internal sealed class Handler(
        AppDbContext context,
        HybridCache cache,
        ICurrentUserService currentUserService)
        : IRequestHandler<Query, Result<List<MemoryResponse>>>
    {
        public async Task<Result<List<MemoryResponse>>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(currentUserService.UserId, out var userId))
            {
                return Result<List<MemoryResponse>>.Unauthorized();
            }

            var cacheKey = MemoryCacheKeys.RecentMemories(userId);

            var result = await cache.GetOrCreateAsync<List<MemoryResponse>>(
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
                    .Take(10)
                    .ToListAsync(cancel),
                tags: [MemoryCacheKeys.MemoryListsTag(userId)],
                cancellationToken: cancellationToken);

            return Result<List<MemoryResponse>>.Success(result);
        }
    }

    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", async (ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new Query(), cancellationToken);
                return ResultExtensions.HandleResult(result);
            })
            .RequireAuthorization(new PermissionAuthorizeAttribute(
                Modules.Memories, Permissions.View));
}
