using AMYL.Api.Data;
using AMYL.Api.Domain.Common;
using AMYL.Api.Features.Memories.Shared;
using AMYL.Api.Shared.Authorization;
using AMYL.Api.Shared.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using System.Security.Claims;
using MemoryEntity = AMYL.Api.Domain.Memory;

namespace AMYL.Api.Features.Memories;

public static class GroupMemoriesByMonth
{
    public sealed record Query(Guid UserId) : IRequest<Result<List<Response>>>;

    public sealed record Response(string MonthLabel, List<MemoryResponse> Memories);

    internal sealed class Handler(
        AppDbContext context,
        HybridCache cache,
        ICurrentUserService currentUserService)
        : IRequestHandler<Query, Result<List<Response>>>
    {
        public async Task<Result<List<Response>>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(currentUserService.UserId, out var userId))
            {
                return Result<List<Response>>.Unauthorized();
            }

            var query = context.Memories
                .Where(memory => memory.UserId == userId)
                .AsNoTracking()
                .OrderByDescending(memory => memory.CreatedAt);

            var cacheKey = MemoryCacheKeys.MemoriesGroupByMonth(userId);

            var result = await cache.GetOrCreateAsync(
                cacheKey,
                async cancel => await GetListing(query, cancel),
                tags: [MemoryCacheKeys.MemoryListsTag(userId)],
                cancellationToken: cancellationToken);

            return Result<List<Response>>.Success(result);
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

    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("/group-by-month", async (
                ClaimsPrincipal claims,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var claimUserId = claims.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(claimUserId, out var userId))
                {
                    return ResultExtensions.HandleResult(
                        Result.Unauthorized("User id not found."));
                }

                var result = await sender.Send(new Query(userId), cancellationToken);
                return ResultExtensions.HandleResult(result);
            })
            .RequireAuthorization(new PermissionAuthorizeAttribute(
                Modules.Memories, Permissions.View));
}
