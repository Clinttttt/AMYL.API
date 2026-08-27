using AMYL.Api.Features.Memories;

namespace AMYL.Api.Features.Memories.GetGroupedByMonth
{
    public record MemoryByMonthDto(
        string MonthLabel,
        List<MemoryDto> memories
        );
}
