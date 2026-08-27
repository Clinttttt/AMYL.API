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

public static class DeleteMemory
{
    public sealed record Command(Guid MemoryId) : IRequest<Result>;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.MemoryId).NotEmpty();
        }
    }

    internal sealed class Handler(
        AppDbContext context,
        HybridCache cache,
        ICurrentUserService currentUserService) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(currentUserService.UserId, out var userId))
            {
                return Result.Unauthorized();
            }

            var memory = await context.Memories
                .FirstOrDefaultAsync(
                    memory => memory.Id == request.MemoryId && memory.UserId == userId,
                    cancellationToken);

            if (memory is null)
            {
                return Result.NotFound($"Memory with id {request.MemoryId} not found.");
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

    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("/delete", async (
                [FromBody] Command command,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(command, cancellationToken);
                return ResultExtensions.HandleResult(result);
            })
            .RequireAuthorization(new PermissionAuthorizeAttribute(
                Modules.Memories, Permissions.Delete));
}
