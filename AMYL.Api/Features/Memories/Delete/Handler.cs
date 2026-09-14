using AMYL.Api.Abstractions.Messaging;
using AMYL.Api.Domain.Common;
using AMYL.Api.Domain.Errors;
using AMYL.Api.Features.Memories.Shared;
using AMYL.Api.Infrastructure.Authentication;
using AMYL.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace AMYL.Api.Features.Memories.Delete;

internal sealed class Handler(
    AppDbContext context,
    HybridCache cache,
    ICurrentUserService currentUserService) : ICommandHandler<Command>
{
    public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(currentUserService.UserId, out var userId))
        {
            return MemoryErrors.Unauthorized;
        }

        // Scoped to the caller: endpoint authorization proves who they are, never which rows they own.
        var memory = await context.Memories
            .FirstOrDefaultAsync(
                memory => memory.Id == request.MemoryId && memory.UserId == userId,
                cancellationToken);

        if (memory is null)
        {
            return MemoryErrors.NotFound;
        }

        context.Memories.Remove(memory);
        await context.SaveChangesAsync(cancellationToken);

        await cache.RemoveAsync(
            MemoryCacheKeys.Memory(userId, request.MemoryId), cancellationToken);
        await cache.RemoveByTagAsync(
            MemoryCacheKeys.MemoryListsTag(userId), cancellationToken);

        return Result.Success();
    }
}
