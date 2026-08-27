using AMYL.Api.Data;
using AMYL.Api.Domain.Common;
using AMYL.Api.Features.Memories.Shared;
using AMYL.Api.Shared.Authorization;
using AMYL.Api.Shared.Extensions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace AMYL.Api.Features.Memories;

public static class ListMemories
{
    public sealed record Query(string? Search, int PageNumber = 1, int PageSize = 10)
        : IRequest<Result<PaginatedList<MemoryResponse>>>;

    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(query => query.PageNumber).GreaterThanOrEqualTo(1);
            RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
        }
    }

    internal sealed class Handler(
        AppDbContext context,
        ICurrentUserService currentUserService,
        HybridCache cache) : IRequestHandler<Query, Result<PaginatedList<MemoryResponse>>>
    {
        public async Task<Result<PaginatedList<MemoryResponse>>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(currentUserService.UserId, out var userId))
            {
                return Result<PaginatedList<MemoryResponse>>.Unauthorized();
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

    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("/list", async (
                ISender sender,
                CancellationToken cancellationToken,
                [FromQuery] string? search,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10) =>
            {
                var result = await sender.Send(
                    new Query(search, pageNumber, pageSize), cancellationToken);
                return ResultExtensions.HandleResult(result);
            })
            .RequireAuthorization(new PermissionAuthorizeAttribute(
                Modules.Memories, Permissions.View));
}
