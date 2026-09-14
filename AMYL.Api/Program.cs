using AMYL.Api.DependencyInjection;
using AMYL.Api.Extensions;
using AMYL.Api.Features.Authentication;
using AMYL.Api.Features.Memories;
using Serilog;
using Serilog.Events;

// Bootstrap logger: without it, a failure during startup — bad connection string,
// missing configuration section — is logged nowhere.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services
        .AddApplication()
        .AddPersistence(builder.Configuration)
        .AddCaching(builder.Configuration)
        .AddApiAuthentication(builder.Configuration)
        .AddApiAuthorization()
        .AddApiRateLimiting()
        .AddApiOpenApi()
        .AddApiCors()
        .AddObservability(builder.Configuration)
        .AddAuthenticationFeature()
        .AddMemoriesFeature();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseExceptionHandler();

    // After exception handling so it observes the final status code, before routing.
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "{RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.##}ms";

        options.GetLevel = (httpContext, _, exception) =>
            exception is not null || httpContext.Response.StatusCode >= 500
                ? LogEventLevel.Error
                : LogEventLevel.Information;
    });

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseCors("Default");
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();

    app.MapHealthChecks("/health");

    // Every IEndpoint, discovered by assembly scan. Adding a slice edits nothing here.
    app.MapEndpoints();

    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "Application terminated unexpectedly");
}
finally
{
    // Buffered sinks lose their last batch — including the fatal record — without this.
    Log.CloseAndFlush();
}

public partial class Program;
