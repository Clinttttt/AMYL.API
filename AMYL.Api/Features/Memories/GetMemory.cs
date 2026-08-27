using AMYL.Api.Data;
using AMYL.Api.Domain.Common;
using AMYL.Api.Features.Memories.Shared;
using AMYL.Api.Shared.Authorization;
using AMYL.Api.Shared.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using System.Security.Claims;

namespace AMYL.Api.Features.Memories;

public static class GetMemory
{
    public sealed record Query(Guid MemoryId, Guid UserId) : IRequest<Result<MemoryResponse>>;

    internal sealed class Handler(AppDbContext context, HybridCache cache)
        : IRequestHandler<Query, Result<MemoryResponse>>
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
                ? Result<MemoryResponse>.NotFound("Memory not found.")
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

    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("/{memoryId:guid}", async (
                [FromRoute] Guid memoryId,
                ISender sender,
                ClaimsPrincipal claims,
                CancellationToken cancellationToken) =>
            {
                var claimUserId = claims.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(claimUserId, out var userId))
                {
                    return ResultExtensions.HandleResult(Result<MemoryResponse>.Unauthorized());
                }

                var result = await sender.Send(new Query(memoryId, userId), cancellationToken);
                return ResultExtensions.HandleResult(result);
            })
            .RequireAuthorization(new PermissionAuthorizeAttribute(
                Modules.Memories, Permissions.View));
}
