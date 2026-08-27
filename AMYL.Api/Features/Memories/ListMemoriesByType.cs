using AMYL.Api.Data;
using AMYL.Api.Domain;
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

public static class ListMemoriesByType
{
    public sealed record Query(
        MemoryType Type,
        string? Search,
        int PageSize = 10,
        int PageNumber = 1) : IRequest<Result<PaginatedList<MemoryResponse>>>;

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

            var cacheKey = MemoryCacheKeys.MemoriesByType(
                userId, request.Type, request.PageNumber, request.PageSize, request.Search);

            var listing = await cache.GetOrCreateAsync<PaginatedList<MemoryResponse>>(
                cacheKey,
                cancel => PaginatedListAsync(
                    userId, request.Type, request.Search,
                    request.PageNumber, request.PageSize, cancel),
                tags: [MemoryCacheKeys.MemoryListsTag(userId)],
                cancellationToken: cancellationToken);

            return Result<PaginatedList<MemoryResponse>>.Success(listing);
        }

        private async ValueTask<PaginatedList<MemoryResponse>> PaginatedListAsync(
            Guid userId,
            MemoryType type,
            string? search,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = context.Memories
                .AsNoTracking()
                .Where(memory => memory.UserId == userId && memory.MemoryType == type)
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

            var searchTrim = search?.Trim();
            query = query.WhereIf(
                !string.IsNullOrWhiteSpace(searchTrim),
                memory => memory.Title!.Contains(searchTrim!));

            return await query.ToPaginatedListAsync(pageNumber, pageSize, cancellationToken);
        }
    }

    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("/type", async (
                [FromQuery] MemoryType type,
                [FromQuery] string? search,
                [FromQuery] int pageSize,
                [FromQuery] int pageNumber,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new Query(type, search, pageSize, pageNumber), cancellationToken);
                return ResultExtensions.HandleResult(result);
            })
            .RequireAuthorization(new PermissionAuthorizeAttribute(
                Modules.Memories, Permissions.View));
}
