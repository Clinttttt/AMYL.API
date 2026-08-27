using Microsoft.Extensions.Caching.Hybrid;

namespace AMYL.Api.Infrastructure.Caching
{
    public static class CachingExtensions
    {
        public static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration)
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
}
