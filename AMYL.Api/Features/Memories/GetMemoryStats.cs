using AMYL.Api.Data;
using AMYL.Api.Domain.Common;
using AMYL.Api.Shared.Authorization;
using AMYL.Api.Shared.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AMYL.Api.Features.Memories;

public static class GetMemoryStats
{
    public sealed record Query(Guid UserId) : IRequest<Result<Response>>;

    public sealed record Response(
        int TotalVideoCount,
        int TotalAudioCount,
        int? TotalImageCount,
        int TotalMemories);

    internal sealed class Handler(AppDbContext context)
        : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var stats = await context.Users
                .AsNoTracking()
                .Where(user => user.Id == request.UserId)
                .Select(user => new Response(
                    TotalVideoCount: user.Memories.Count(memory => memory.VideoUrl != null),
                    TotalAudioCount: user.Memories.Count(memory => memory.AudioUrl != null),
                    TotalImageCount: user.Memories.Count(memory => memory.ImageUrl != null),
                    TotalMemories: user.Memories.Count))
                .FirstOrDefaultAsync(cancellationToken);

            return stats is null
                ? Result<Response>.Failure("User not found")
                : Result<Response>.Success(stats);
        }
    }

    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("/stats", async (
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
