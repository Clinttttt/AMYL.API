using AMYL.Api.Domain;

namespace AMYL.Api.Features.Memories.Shared;

/// <summary>
/// Cache keys for the Memories feature. Every key includes the user id so
/// entries can never leak across users.
/// </summary>
public static class MemoryCacheKeys
{
    public static string Memory(Guid userId, Guid memoryId) =>
        $"user:{userId}:memory:{memoryId}";

    public static string RecentMemories(Guid userId) =>
        $"user:{userId}:memories:recent";

    public static string MemoryStats(Guid userId) =>
        $"user:{userId}:memories:stats";

    public static string MemoryTag(Guid userId) =>
        $"user:{userId}:memories";

    public static string MemoryList(
        Guid userId,
        int pageNumber,
        int pageSize,
        string? search)
    {
        var normalizedSearch = NormalizeSearch(search);

        return $"user:{userId}:memories:list:" +
               $"page:{pageNumber}:" +
               $"size:{pageSize}:" +
               $"search:{normalizedSearch}";
    }

    public static string MemoriesGroupByMonth(Guid userId) =>
        $"user:{userId}:memories:groupbymonth";

    public static string MemoriesByType(
        Guid userId,
        MemoryType memoryType,
        int pageNumber,
        int pageSize,
        string? search)
    {
        var normalizedSearch = NormalizeSearch(search);

        return $"user:{userId}:memories:type:{memoryType}:" +
               $"page:{pageNumber}:" +
               $"size:{pageSize}:" +
               $"search:{normalizedSearch}";
    }

    public static string MemoryListsTag(Guid userId) =>
        $"user:{userId}:memories:lists";

    private static string NormalizeSearch(string? search) =>
        string.IsNullOrWhiteSpace(search)
            ? "none"
            : search.Trim().ToLowerInvariant();
}
