namespace AMYL.Api.Features.Memories;

/// <summary>
/// Registers the providers this feature owns. Routing is no longer listed here —
/// each slice's <c>Endpoint</c> is discovered by assembly scan.
/// </summary>
public static class MemoriesModule
{
    public static IServiceCollection AddMemoriesFeature(this IServiceCollection services)
    {
        services.AddScoped<IFileStorage, MemoryFileStorage>();

        return services;
    }
}
