using Microsoft.Extensions.Caching.Hybrid;

namespace AMYL.Api.DependencyInjection;

public static class CachingServices
{
    /// <summary>
    /// Redis is optional. When no Redis connection string is configured the
    /// application still works using HybridCache's local memory tier only.
    /// </summary>
    public static IServiceCollection AddCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisConnection = configuration.GetConnectionString("Redis");

        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "amyl:";
            });
        }

        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(5),
                LocalCacheExpiration = TimeSpan.FromMinutes(1),
            };
        });

        return services;
    }
}
