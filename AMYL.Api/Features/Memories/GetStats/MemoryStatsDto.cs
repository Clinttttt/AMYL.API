
namespace AMYL.Api.Features.Memories.GetStats
{
    public sealed record MemoryStatsDto(
     int TotalVideoCount,
     int TotalAudioCount,
     int? TotalImageCount,
     int TotalMemories);
}
