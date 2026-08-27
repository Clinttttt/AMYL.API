namespace AMYL.Api.Features.Memories;

public static class MemoriesModule
{
    /// <summary>
    /// Registers the providers this feature owns.
    /// </summary>
    public static IServiceCollection AddMemoriesFeature(this IServiceCollection services)
    {
        services.AddScoped<IFileStorage, MemoryFileStorage>();

        return services;
    }

    public static void MapMemories(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/memories")
            .WithTags("Memory")
            .RequireAuthorization();

        CreateMemory.Map(group);
        UpdateMemory.Map(group);
        DeleteMemory.Map(group);
        GroupMemoriesByMonth.Map(group);
        GetMemory.Map(group);
        ListMemories.Map(group);
        GetMemoryStats.Map(group);
        ListMemoriesByType.Map(group);
        ListRecentMemories.Map(group);
    }
}
