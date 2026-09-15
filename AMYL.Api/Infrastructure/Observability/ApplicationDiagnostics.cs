using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace AMYL.Api.Infrastructure.Observability;

/// <summary>
/// The single <see cref="ActivitySource"/> and <see cref="Meter"/> for this application.
/// <para>
/// <see cref="SourceName"/> must be passed to <c>AddSource</c> and <c>AddMeter</c> in
/// <c>ObservabilityServices</c>. Without it, <c>StartActivity</c> returns <c>null</c>,
/// the behaviours keep working, and no spans are ever exported.
/// </para>
/// </summary>
public static class ApplicationDiagnostics
{
    public const string SourceName = "AMYL.Api";

    public static readonly ActivitySource ActivitySource = new(SourceName);

    public static readonly Meter Meter = new(SourceName);

    /// <summary>Counts handled commands and queries, tagged by use case and outcome.</summary>
    public static readonly Counter<long> MessagesHandled =
        Meter.CreateCounter<long>("amyl.messages.handled");
}

/// <summary>
/// The closed set of values for the <c>outcome</c> dimension. Kept small and bounded on
/// purpose: a metric dimension with unbounded values is a cost and cardinality problem.
/// </summary>
internal static class TelemetryOutcomes
{
    public const string Success = "success";
    public const string Failure = "failure";
    public const string Exception = "exception";
    public const string Cancelled = "cancelled";
}

/// <summary>
/// Tag and property names. Constants rather than literals because a typo in a tag name does
/// not fail — it silently creates a second dimension and quietly breaks every dashboard
/// built on the first.
/// </summary>
internal static class TelemetryTags
{
    // Metric dimensions
    public const string UseCase = "use_case";
    public const string Outcome = "outcome";

    // Span tags — dotted, per OpenTelemetry attribute convention
    public const string ResultOutcome = "result.outcome";
    public const string ErrorCode = "error.code";
    public const string ExceptionType = "exception.type";

    // Log scope property — PascalCase, per Serilog/ILogger property convention
    public const string UseCaseProperty = "UseCase";
}
