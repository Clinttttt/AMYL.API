using AMYL.Api.Data;
using AMYL.Api.Domain;
using AMYL.Api.Domain.Common;
using AMYL.Api.Features.Memories.Shared;
using AMYL.Api.Shared.Authorization;
using AMYL.Api.Shared.Extensions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;
using System.Linq.Expressions;
using MemoryEntity = AMYL.Api.Domain.Memory;

namespace AMYL.Api.Features.Memories;

public static class CreateMemory
{
    // NOTE: IFormFile on a MediatR command is a known migration item
    // (design_2 section 19). Bind multipart input at the endpoint and map it to
    // a feature-owned model before sending the command.
    public sealed record Command(
        string Title,
        string Description,
        MemoryType MemoryType,
        IFormFile VideoUrl,
        IFormFile? ImageUrl,
        IFormFile? AudioUrl,
        string? SecretNote) : IRequest<Result>;

    public sealed class Validator : AbstractValidator<Command>
    {
        private const long MaximumFileSize = 10 * 1024 * 1024;

        public Validator()
        {
            AddMediaRule(command => command.VideoUrl, "Video");
            AddMediaRule(command => command.ImageUrl, "Image");
            AddMediaRule(command => command.AudioUrl, "Audio");
        }

        private void AddMediaRule(
            Expression<Func<Command, IFormFile?>> property,
            string mediaName,
            bool isRequired = false)
        {
            var rule = RuleFor(property).Cascade(CascadeMode.Stop);

            if (isRequired)
            {
                rule.NotEmpty().WithMessage($"{mediaName} is required.");
            }

            rule
                .Must(file => file is null || file.Length <= MaximumFileSize)
                .WithMessage($"{mediaName} must not exceed 10 MB.");
        }
    }

    internal sealed class Handler(
        AppDbContext context,
        IFileStorage fileStorage,
        ICurrentUserService currentUserService,
        HybridCache cache,
        TimeProvider timeProvider) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(currentUserService.UserId, out var userId))
            {
                return Result.Unauthorized();
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

            var memory = MemoryEntity.Create(
                request.Title,
                request.Description,
                timeProvider.GetUtcNow().UtcDateTime,
                request.MemoryType,
                imageUrl,
                videoUrl,
                audioUrl,
                request.SecretNote,
                userId: userId);

            await context.Memories.AddAsync(memory, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            await cache.RemoveByTagAsync(MemoryCacheKeys.MemoryListsTag(userId), cancellationToken);

            return Result.Success();
        }
    }

    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("/create", async (
                [FromBody] Command command,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(command, cancellationToken);
                return ResultExtensions.HandleResult(result);
            })
            .RequireRateLimiting(RateLimitPolicies.General)
            .RequireAuthorization(new PermissionAuthorizeAttribute(
                Modules.Memories, Permissions.Create));
}
