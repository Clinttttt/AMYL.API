using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace AMYL.Api.Web.Extensions
{
    public static class OpenTelemetryExtensions
    {
        public static IServiceCollection AddApiOpenTelemetry(this IServiceCollection services)
        {
            services.AddOpenTelemetry()
                .ConfigureResource(resource =>
                resource.AddService("AMYL.Api"))
                .WithTracing(trace => trace.AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                }).AddOtlpExporter());
            return services;
        }
    }
}
