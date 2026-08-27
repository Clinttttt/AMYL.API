using AMYL.Api.Web.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace AMYL.Api.Web.Extensions
{
    public static class RateLimiterExtensions
    {
        public static IServiceCollection RateLimiter(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.AddSlidingWindowLimiter(RateLimitPolicies.General, opt =>
                {
                    opt.PermitLimit = 100;
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.SegmentsPerWindow = 2;
                });
                options.AddSlidingWindowLimiter(RateLimitPolicies.Login, opt =>
                {
                    opt.PermitLimit = 5;
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.SegmentsPerWindow = 2;
                }
                );
            });
            return services;
        }

    }
}
