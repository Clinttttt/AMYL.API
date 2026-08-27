namespace AMYL.Api.Features.Memories.Shared;

/// <summary>
/// Feature-local response shared by several sibling Memories slices.
/// Still inside the feature boundary - not a global DTO folder.
/// </summary>
public sealed record MemoryResponse(
    Guid MemoryId,
    string? Title,
    string? Description,
    string? ImageUrl,
    string? VideoUrl,
    string? AudioUrl,
    DateTime CreatedAt);
