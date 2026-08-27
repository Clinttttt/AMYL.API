using AMYL.Api.Domain.Common;

namespace AMYL.Api.Domain;

public class Memory : AuditableEntity
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MemoryType MemoryType { get; set; }
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string? AudioUrl { get; set; }
    public string? SecretNote { get; set; }
    public Users? Users { get; set; }

    public Memory() { }

    public static Memory Create(
        string title,
        string? description,
        DateTime createdAt,
        MemoryType memoryType,
        string? imageUrl,
        string? videoUrl,
        string? audioUrl,
        string? secretNote,
        Guid userId)
    {
        return new Memory
        {
            Title = title,
            Description = description,
            CreatedAt = createdAt,
            MemoryType = memoryType,
            ImageUrl = imageUrl,
            VideoUrl = videoUrl,
            AudioUrl = audioUrl,
            SecretNote = secretNote,
            UserId = userId,
        };
    }

    public static void Update(
        Memory memory,
        string title,
        string? description,
        MemoryType? memoryType,
        string? imageUrl,
        string? videoUrl,
        string? audioUrl)
    {
        memory.Title = title ?? memory.Title;
        memory.Description = description ?? memory.Description;
        memory.MemoryType = memoryType ?? memory.MemoryType;
        memory.ImageUrl = imageUrl ?? memory.ImageUrl;
        memory.VideoUrl = videoUrl ?? memory.VideoUrl;
        memory.AudioUrl = audioUrl ?? memory.AudioUrl;
    }
}
