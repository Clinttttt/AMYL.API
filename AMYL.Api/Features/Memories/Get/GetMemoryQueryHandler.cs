using AMYL.Api.Shared.Abstractions;
using AMYL.Api.Shared.Caching;
using AMYL.Api.Infrastructure.Persistence;
using AMYL.Api.Features.Memories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace AMYL.Api.Features.Memories.Get
{
    public class GetMemoryQueryHandler(AppDbContext context, HybridCache cache) : IRequestHandler<GetMemoryQuery, Result<MemoryDto>>
    {
        public async Task<Result<MemoryDto>> Handle(GetMemoryQuery request, CancellationToken cancellationToken)
        {

            var cacheKeys = CacheKeys.Memory(request.UserId, request.MemoryId);

            var memory = await cache.GetOrCreateAsync<MemoryDto?>(cacheKeys,
                cancel => FindMemoryAsync(
                    request.MemoryId,
                    request.UserId,
                    cancel),
                tags: [CacheKeys.MemoryTag(request.UserId)],
                cancellationToken: cancellationToken
                );

            return memory is null
                ? Result<MemoryDto>.NotFound("Memory not found.")
                : Result<MemoryDto>.Success(memory);
        }
        private async ValueTask<MemoryDto?> FindMemoryAsync(Guid MemoryId, Guid UserId, CancellationToken cancellationToken = default)
        {
            return await context.Memories.AsNoTracking()
               .Where(s => s.Id == MemoryId && s.UserId == UserId)
               .Select(s => new MemoryDto(
                   MemoryId: s.Id,
                   Title: s.Title,
                   Description: s.Description,
                   ImageUrl: s.ImageUrl,
                   VideoUrl: s.VideoUrl,
                   AudioUrl: s.AudioUrl,
                   CreatedAt: s.CreateAt
                   )).FirstOrDefaultAsync(cancellationToken);
        }
    }
}
