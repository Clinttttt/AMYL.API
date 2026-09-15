using System.Diagnostics;
using AMYL.Api.Behaviors;
using AMYL.Api.Domain.Common;
using AMYL.Api.Infrastructure.Observability;
using MediatR;
using Create = AMYL.Tests.Fakes.Create;
using Delete = AMYL.Tests.Fakes.Delete;
using Microsoft.Extensions.Logging.Abstractions;

namespace AMYL.Tests;

/// <summary>
/// The span name and the <c>use_case</c> metric dimension must identify the slice. With a
/// folder per use case every request type is named <c>Command</c> or <c>Query</c>, so naming
/// spans from the type alone collapses every slice into two buckets. These tests fail if that
/// regresses.
/// </summary>
public sealed class LoggingBehaviorTests
{
    [Fact]
    public async Task Handle_NamesTheActivityAfterTheUseCase_NotTheTypeName()
    {
        var activities = new List<Activity>();
        using var listener = Listen(activities);

        var behavior = new LoggingBehavior<Create.Command, Result>(
            NullLogger<LoggingBehavior<Create.Command, Result>>.Instance);

        await behavior.Handle(
            new Create.Command(),
            _ => Task.FromResult(Result.Success()),
            CancellationToken.None);

        var activity = Assert.Single(activities);

        Assert.Equal("Create.Command", activity.OperationName);
        Assert.NotEqual("Command", activity.OperationName);
    }

    [Fact]
    public async Task Handle_DistinguishesTwoSlicesWhoseRequestTypesShareAName()
    {
        var activities = new List<Activity>();
        using var listener = Listen(activities);

        await new LoggingBehavior<Create.Command, Result>(
                NullLogger<LoggingBehavior<Create.Command, Result>>.Instance)
            .Handle(new Create.Command(), _ => Task.FromResult(Result.Success()), CancellationToken.None);

        await new LoggingBehavior<Delete.Command, Result>(
                NullLogger<LoggingBehavior<Delete.Command, Result>>.Instance)
            .Handle(new Delete.Command(), _ => Task.FromResult(Result.Success()), CancellationToken.None);

        Assert.Equal(
            ["Create.Command", "Delete.Command"],
            activities.Select(activity => activity.OperationName));
    }

    [Fact]
    public async Task Handle_OnExpectedFailure_LeavesSpanStatusUnset()
    {
        var activities = new List<Activity>();
        using var listener = Listen(activities);

        var behavior = new LoggingBehavior<Create.Command, Result>(
            NullLogger<LoggingBehavior<Create.Command, Result>>.Instance);

        await behavior.Handle(
            new Create.Command(),
            _ => Task.FromResult(Result.Failure("nope")),
            CancellationToken.None);

        var activity = Assert.Single(activities);

        // A Result failure is the application working as designed, not a fault.
        Assert.Equal(ActivityStatusCode.Unset, activity.Status);
        Assert.Equal("failure", activity.GetTagItem("result.outcome"));
    }

    [Fact]
    public async Task Handle_OnException_SetsSpanStatusToErrorAndRethrows()
    {
        var activities = new List<Activity>();
        using var listener = Listen(activities);

        var behavior = new LoggingBehavior<Create.Command, Result>(
            NullLogger<LoggingBehavior<Create.Command, Result>>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() => behavior.Handle(
            new Create.Command(),
            _ => throw new InvalidOperationException("boom"),
            CancellationToken.None));

        var activity = Assert.Single(activities);

        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Equal(nameof(InvalidOperationException), activity.GetTagItem("exception.type"));
    }

    private static ActivityListener Listen(List<Activity> collected)
    {
        var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == ApplicationDiagnostics.SourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStopped = collected.Add
        };

        ActivitySource.AddActivityListener(listener);

        return listener;
    }
}
