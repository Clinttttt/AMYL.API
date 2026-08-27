using AMYL.Api.Shared.Abstractions;
using AMYL.Api.Shared.Caching;
using AMYL.Api.Features.Memories;
using AMYL.Api.Shared.Security;
using AMYL.Api.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace AMYL.Api.Features.Memories.Update
{
    public class UpdateMemoryCommandHandler(AppDbContext context, IFileService fileService, HybridCache cache, ICurrentUserService currentUserService) : IRequestHandler<UpdateMemoryCommand, Result>
    {
        public async Task<Result> Handle(UpdateMemoryCommand request, CancellationToken cancellationToken)
        {
            var Id = currentUserService.UserId;
            if (!Guid.TryParse(Id, out var id))
                return Result.Unauthorized();

            var memory = await context.Memories
                .FirstOrDefaultAsync(
                    s => s.Id == request.MemoryId && s.UserId == id,
                    cancellationToken);

            if (memory is null)
                return Result.NotFound("Memory not found");

            var videoUrl = request.VideoUrl != null
                ? await fileService.UploadFile(request.VideoUrl, cancellationToken) : null;
            var ImageUrl = request.ImageUrl != null
              ? await fileService.UploadFile(request.ImageUrl, cancellationToken) : null;
            var AudioUrl = request.AudioUrl != null
              ? await fileService.UploadFile(request.AudioUrl, cancellationToken) : null;

            AMYL.Api.Shared.Domain.Entities.Memory.Update(
                memory,
                request.Title,
                request.Description,
                request.MemoryType,
                ImageUrl,
                videoUrl,
                AudioUrl
            );
            await context.SaveChangesAsync(cancellationToken);
            await cache.RemoveAsync(CacheKeys.Memory(id, request.MemoryId), cancellationToken);
            await cache.RemoveByTagAsync(CacheKeys.MemoryListsTag(id), cancellationToken);
            return Result.Success();
        }
    }
}
