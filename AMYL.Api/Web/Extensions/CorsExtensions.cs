namespace AMYL.Api.Web.Extensions
{
    public static class CorsExtensions
    {
        public static IServiceCollection AddApiCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("Default", policy =>
                {
                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();
                });
            });
            return services;
        }
    }
}
