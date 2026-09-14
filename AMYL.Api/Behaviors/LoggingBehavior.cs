using System.Diagnostics;
using AMYL.Api.Domain.Common;
using AMYL.Api.Infrastructure.Observability;
using MediatR;

namespace AMYL.Api.Behaviors;

/// <summary>
/// One span, one structured log line, and one metric per command or query — including when
/// the handler throws. Registered outermost so a request rejected by
/// <see cref="ValidationBehavior{TRequest,TResponse}"/> is still observed; that is the event
/// you need when a client reports an unexplained 400.
/// </summary>
/// <remarks>
/// Expected failures and faults are kept distinct, because conflating them makes dashboards
/// useless:
/// <list type="bullet">
/// <item>a <see cref="Result"/> failure is the application working as designed — it logs at
/// Information and leaves the span status unset, carrying <c>error.code</c> as a tag;</item>
/// <item>an exception is a fault — it sets <see cref="ActivityStatusCode.Error"/> and rethrows.</item>
/// </list>
/// The exception itself is <b>not</b> logged here. <c>GlobalExceptionHandler</c> owns that, and
/// logging in both places produces two records of one fault.
/// </remarks>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string useCase = UseCaseName();

        using Activity? activity = ApplicationDiagnostics.ActivitySource
            .StartActivity(useCase, ActivityKind.Internal);

        // BeginScope rather than Serilog's LogContext: the same structured properties,
        // without coupling this behaviour to a specific logging provider.
        using IDisposable? scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["UseCase"] = useCase
        });

        long start = Stopwatch.GetTimestamp();

        try
        {
            TResponse response = await next(cancellationToken);

            double elapsedMs = Stopwatch.GetElapsedTime(start).TotalMilliseconds;

            if (response.IsSuccess)
            {
                activity?.SetStatus(ActivityStatusCode.Ok);

                logger.LogInformation(
                    "Handled {UseCase} in {ElapsedMs:0.##}ms",
                    useCase, elapsedMs);

                Record(useCase, "success");
            }
            else
            {
                string errorCode = response.Error?.Code ?? "unknown";

                // Deliberately not ActivityStatusCode.Error: the application behaved as
                // designed. Marking expected failures as errors turns every rejected
                // form field into a red trace.
                activity?.SetTag("result.outcome", "failure");
                activity?.SetTag("error.code", errorCode);

                logger.LogInformation(
                    "Rejected {UseCase} with {ErrorCode} in {ElapsedMs:0.##}ms",
                    useCase, errorCode, elapsedMs);

                Record(useCase, "failure");
            }

            return response;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // The caller went away. Not a fault, and not worth an error span.
            activity?.SetTag("result.outcome", "cancelled");

            logger.LogInformation(
                "Cancelled {UseCase} after {ElapsedMs:0.##}ms",
                useCase, Stopwatch.GetElapsedTime(start).TotalMilliseconds);

            Record(useCase, "cancelled");

            throw;
        }
        catch (Exception exception)
        {
            // Telemetry only. GlobalExceptionHandler logs the exception and shapes the response.
            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            activity?.SetTag("exception.type", exception.GetType().FullName);

            Record(useCase, "exception");

            throw;   // `throw;` not `throw exception;` — preserves the original stack trace.
        }
    }

    private static void Record(string useCase, string outcome) =>
        ApplicationDiagnostics.MessagesHandled.Add(
            1,
            new KeyValuePair<string, object?>("use_case", useCase),
            new KeyValuePair<string, object?>("outcome", outcome));

    /// <summary>
    /// "Create.Command" for a folder-per-use-case slice, falling back to the type name
    /// for anything still nested in a single-file slice.
    /// </summary>
    private static string UseCaseName()
    {
        Type type = typeof(TRequest);
        string? owner = type.DeclaringType?.Name
                        ?? type.Namespace?.Split('.').LastOrDefault();

        return owner is null ? type.Name : $"{owner}.{type.Name}";
    }
}
