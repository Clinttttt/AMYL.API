using System.Diagnostics;
using AMYL.Api.Domain.Common;
using AMYL.Api.Infrastructure.Observability;
using MediatR;

namespace AMYL.Api.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        string useCase = UseCaseName();

        using var activity =
            ApplicationDiagnostics.ActivitySource.StartActivity(useCase);

        // Attaches UseCase to every log line the handler itself writes, not just this one.
        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            [TelemetryTags.UseCaseProperty] = useCase
        });

        long start = Stopwatch.GetTimestamp();

        try
        {
            TResponse response = await next(cancellationToken);

            string outcome = response.IsSuccess
                ? TelemetryOutcomes.Success
                : TelemetryOutcomes.Failure;

            activity?.SetTag(TelemetryTags.ResultOutcome, outcome);

            if (!response.IsSuccess)
                activity?.SetTag(TelemetryTags.ErrorCode, response.Error?.Code);

            logger.LogInformation(
                "Handled {UseCase} with {Outcome} in {ElapsedMs:0.##}ms",
                useCase,
                outcome,
                Stopwatch.GetElapsedTime(start).TotalMilliseconds);

            Record(useCase, outcome);

            return response;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // The caller went away. Not a fault, so it must not colour the error rate.
            activity?.SetTag(TelemetryTags.ResultOutcome, TelemetryOutcomes.Cancelled);

            Record(useCase, TelemetryOutcomes.Cancelled);

            throw;
        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            activity?.SetTag(TelemetryTags.ExceptionType, exception.GetType().Name);

            Record(useCase, TelemetryOutcomes.Exception);

            throw;
        }
    }

    /// <summary>
    /// "Create.Command" rather than "Command". With a folder per use case every request type
    /// is named Command or Query, so the type name alone collapses every slice into two
    /// buckets and makes both the span name and the use_case metric dimension useless.
    /// </summary>
    private static string UseCaseName()
    {
        Type type = typeof(TRequest);
        string? owner = type.DeclaringType?.Name
                        ?? type.Namespace?.Split('.').LastOrDefault();

        return owner is null ? type.Name : $"{owner}.{type.Name}";
    }

    private static void Record(string useCase, string outcome) =>
        ApplicationDiagnostics.MessagesHandled.Add(
            1,
            new(TelemetryTags.UseCase, useCase),
            new(TelemetryTags.Outcome, outcome));
}