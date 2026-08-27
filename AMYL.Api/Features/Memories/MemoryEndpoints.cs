using AMYL.Api.Features.Memories.Create;
using AMYL.Api.Features.Memories.Delete;
using AMYL.Api.Features.Memories.Get;
using AMYL.Api.Features.Memories.GetByType;
using AMYL.Api.Features.Memories.GetGroupedByMonth;
using AMYL.Api.Features.Memories.GetRecent;
using AMYL.Api.Features.Memories.GetStats;
using AMYL.Api.Features.Memories.List;
using AMYL.Api.Features.Memories.Update;

namespace AMYL.Api.Features.Memories;

public static class MemoryEndpoints
{
    public static void MapMemoryEndpoints(this WebApplication application)
    {
        var group = application
            .MapGroup("/api/memories")
            .WithTags("Memory")
            .RequireAuthorization();

        group.MapCreateMemoryCommand();
        group.MapUpdateMemoryCommand();
        group.MapDeleteMemoryCommand();
        group.MapGetMemoriesGroupedByMonthQuery();
        group.MapMemoryQuery();
        group.MapListMemoryQuery();
        group.MapGetMemoryStatsQuery();
        group.MapGetMemoriesByTypeQuery();
        group.MapGetRecentMemoriesQuery();
    }
}
