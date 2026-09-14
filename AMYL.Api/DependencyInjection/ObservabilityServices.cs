using AMYL.Api.Infrastructure.Observability;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace AMYL.Api.DependencyInjection;

public static class ObservabilityServices
{
    /// <summary>
    /// Serilog for logs, OpenTelemetry for traces and metrics, both exported over OTLP so
    /// every signal shares one TraceId. Point them somewhere with OTEL_EXPORTER_OTLP_ENDPOINT
    /// (defaults to http://localhost:4317).
    /// </summary>
    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSerilog((provider, logger) => logger
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(provider)
            .Enrich.FromLogContext());

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                serviceName: ApplicationDiagnostics.SourceName,
                serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString()))
            .WithTracing(tracing => tracing
                // Without AddSource, LoggingBehavior's StartActivity returns null and no
                // command spans are ever exported. The behaviour keeps working silently.
                .AddSource(ApplicationDiagnostics.SourceName)
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.Filter = context =>
                        !context.Request.Path.StartsWithSegments("/health");
                })
                .AddNpgsql()
                .AddOtlpExporter())
            .WithMetrics(metrics => metrics
                .AddMeter(ApplicationDiagnostics.SourceName)
                .AddAspNetCoreInstrumentation()
                .AddOtlpExporter());

        return services;
    }
}
