namespace AMYL.Api.Features.Memories.GetStats;

public sealed record Response(
    int TotalVideoCount,
    int TotalAudioCount,
    int? TotalImageCount,
    int TotalMemories);
