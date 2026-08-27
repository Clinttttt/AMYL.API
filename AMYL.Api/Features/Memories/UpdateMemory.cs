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
using MemoryEntity = AMYL.Api.Domain.Memory;

namespace AMYL.Api.Features.Memories;

public static class UpdateMemory
{
    public sealed record Command(
        Guid MemoryId,
        string Title,
        string Description,
        MemoryType MemoryType,
        IFormFile? VideoUrl,
        IFormFile? ImageUrl,
        IFormFile? AudioUrl) : IRequest<Result>;

    public sealed class Validator : AbstractValidator<Command>
    {
        private const long MaximumFileSize = 10 * 1024 * 1024;

        public Validator()
        {
            RuleFor(command => command.MemoryId).NotEmpty();
            RuleFor(command => command.Title).NotEmpty().MaximumLength(200);
            RuleFor(command => command.Description).MaximumLength(4_000);

            RuleFor(command => command.VideoUrl)
                .Must(file => file is null || file.Length <= MaximumFileSize)
                .WithMessage("Video must not exceed 10 MB.");

            RuleFor(command => command.ImageUrl)
                .Must(file => file is null || file.Length <= MaximumFileSize)
                .WithMessage("Image must not exceed 10 MB.");

            RuleFor(command => command.AudioUrl)
                .Must(file => file is null || file.Length <= MaximumFileSize)
                .WithMessage("Audio must not exceed 10 MB.");
        }
    }

    internal sealed class Handler(
        AppDbContext context,
        IFileStorage fileStorage,
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
                return Result.NotFound("Memory not found");
            }

            var videoUrl = request.VideoUrl != null
                ? await fileStorage.UploadFile(request.VideoUrl, cancellationToken)
                : null;
            var imageUrl = request.ImageUrl != null
                ? await fileStorage.UploadFile(request.ImageUrl, cancellationToken)
                : null;
            var audioUrl = request.AudioUrl != null
                ? await fileStorage.UploadFile(request.AudioUrl, cancellationToken)
                : null;

            MemoryEntity.Update(
                memory,
                request.Title,
                request.Description,
                request.MemoryType,
                imageUrl,
                videoUrl,
                audioUrl);

            await context.SaveChangesAsync(cancellationToken);
            await cache.RemoveAsync(
                MemoryCacheKeys.Memory(userId, request.MemoryId), cancellationToken);
            await cache.RemoveByTagAsync(
                MemoryCacheKeys.MemoryListsTag(userId), cancellationToken);

            return Result.Success();
        }
    }

    public static void Map(RouteGroupBuilder group) =>
        group.MapPatch("/update", async (
                [FromBody] Command command,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(command, cancellationToken);
                return ResultExtensions.HandleResult(result);
            })
            .RequireAuthorization(new PermissionAuthorizeAttribute(
                Modules.Memories, Permissions.Update));
}
