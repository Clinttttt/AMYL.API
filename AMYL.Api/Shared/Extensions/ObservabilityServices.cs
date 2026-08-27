using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace AMYL.Api.Shared.Extensions;

public static class ObservabilityServices
{
    public static IServiceCollection AddObservability(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("AMYL.Api"))
            .WithTracing(trace => trace
                .AddAspNetCoreInstrumentation(options => options.RecordException = true)
                .AddOtlpExporter());

        return services;
    }
}
