using Microsoft.AspNetCore.RateLimiting;

namespace AMYL.Api.Shared.Extensions;

public static class RateLimitPolicies
{
    public const string General = nameof(General);
    public const string Login = nameof(Login);
}

public static class RateLimitingServices
{
    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddSlidingWindowLimiter(RateLimitPolicies.General, limiter =>
            {
                limiter.PermitLimit = 100;
                limiter.Window = TimeSpan.FromMinutes(1);
                limiter.SegmentsPerWindow = 2;
            });

            options.AddSlidingWindowLimiter(RateLimitPolicies.Login, limiter =>
            {
                limiter.PermitLimit = 5;
                limiter.Window = TimeSpan.FromMinutes(1);
                limiter.SegmentsPerWindow = 2;
            });
        });

        return services;
    }
}
