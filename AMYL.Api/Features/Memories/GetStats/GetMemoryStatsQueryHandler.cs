using AMYL.Api.Shared.Abstractions;
using AMYL.Api.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AMYL.Api.Features.Memories.GetStats
{
    public class GetMemoryStatsQueryHandler(AppDbContext context) : IRequestHandler<GetMemoryStatsQuery, Result<MemoryStatsDto>>
    {
        public async Task<Result<MemoryStatsDto>> Handle(GetMemoryStatsQuery request, CancellationToken cancellationToken)
        {
            var query = await context.Users
                 .AsNoTracking()
                 .Where(s => s.Id == request.UserId)
                 .Select(m => new MemoryStatsDto
                 (
                     TotalVideoCount: m.Memories.Count(s => s.VideoUrl != null),
                     TotalAudioCount: m.Memories.Count(s => s.AudioUrl != null),
                     TotalImageCount: m.Memories.Count(s => s.ImageUrl != null),
                     TotalMemories: m.Memories.Count
                 )).FirstOrDefaultAsync(cancellationToken);

            if (query is null)
                return Result<MemoryStatsDto>.Failure("User not found");

            return Result<MemoryStatsDto>.Success(query);

        }
    }
}
