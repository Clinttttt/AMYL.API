namespace AMYL.Api.Features.Memories
{
    public sealed record MemoryDto
    (
         Guid MemoryId,
         string? Title,
         string? Description,
         string? ImageUrl,
         string? VideoUrl,
         string? AudioUrl,
         DateTime CreatedAt
    );
}
