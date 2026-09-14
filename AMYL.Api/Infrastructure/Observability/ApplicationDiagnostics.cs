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
