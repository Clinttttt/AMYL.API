using AMYL.Api.Abstractions.Messaging;
using AMYL.Api.Domain.Common;
using AMYL.Api.Domain.Errors;
using AMYL.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AMYL.Api.Features.Memories.GetStats;

internal sealed class Handler(AppDbContext context) : IQueryHandler<Query, Response>
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
            ? UserErrors.NotFound
            : Result<Response>.Success(stats);
    }
}
