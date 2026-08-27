using AMYL.Api.Shared.Abstractions;
using AMYL.Api.Shared.Caching;
using AMYL.Api.Shared.Security;
using AMYL.Api.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using System.Diagnostics.Contracts;

namespace AMYL.Api.Features.Memories.Delete
{
    public class DeleteMemoryCommandHandler(AppDbContext context, HybridCache cache, ICurrentUserService currentUserService) : IRequestHandler<DeleteMemoryCommand, Result>
    {
        public async Task<Result> Handle(DeleteMemoryCommand request, CancellationToken cancellationToken)
        {
            var Id = currentUserService.UserId;
            if (!Guid.TryParse(Id, out var id))
                return Result.Unauthorized();

            var memory = await context.Memories
                .FirstOrDefaultAsync(
                    s => s.Id == request.MemoryId && s.UserId == id,
                    cancellationToken);

            if (memory is null)
            {
                return Result.NotFound($"Memory with id {request.MemoryId} not found.");
            }

            context.Memories.Remove(memory);
            await context.SaveChangesAsync(cancellationToken);
            await cache.RemoveAsync(CacheKeys.Memory(id, request.MemoryId), cancellationToken);
            await cache.RemoveByTagAsync(CacheKeys.MemoryListsTag(id), cancellationToken);

            return Result.Success();
        }
    }
}
