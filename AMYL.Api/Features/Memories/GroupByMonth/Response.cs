using AMYL.Api.Features.Memories.Shared;

namespace AMYL.Api.Features.Memories.GroupByMonth;

public sealed record Response(string MonthLabel, List<MemoryResponse> Memories);
