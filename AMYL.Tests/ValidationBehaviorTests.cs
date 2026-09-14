using AMYL.Api.Abstractions.Messaging;
using AMYL.Api.Behaviors;
using AMYL.Api.Domain.Common;
using FluentValidation;

namespace AMYL.Tests;

/// <summary>
/// The three invocation-count tests are mandatory: this behaviour sits in front of every
/// command, and awaiting <c>next</c> twice would execute each command twice while still
/// returning correct responses. The fourth test pins the contract that a validation failure
/// is returned as a failed <see cref="Result"/> rather than thrown.
/// </summary>
public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WithoutValidators_InvokesHandlerOnce()
    {
        var behavior = new ValidationBehavior<TestCommand, Result>([]);
        var invocationCount = 0;

        var result = await behavior.Handle(
            new TestCommand("valid"),
            _ =>
            {
                invocationCount++;
                return Task.FromResult(Result.Success());
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, invocationCount);
    }

    [Fact]
    public async Task Handle_WithValidRequest_InvokesHandlerOnce()
    {
        var behavior = new ValidationBehavior<TestCommand, Result>([NotEmptyValidator()]);
        var invocationCount = 0;

        var result = await behavior.Handle(
            new TestCommand("valid"),
            _ =>
            {
                invocationCount++;
                return Task.FromResult(Result.Success());
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, invocationCount);
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_DoesNotInvokeHandler()
    {
        var behavior = new ValidationBehavior<TestCommand, Result>([NotEmptyValidator()]);
        var invocationCount = 0;

        var result = await behavior.Handle(
            new TestCommand(string.Empty),
            _ =>
            {
                invocationCount++;
                return Task.FromResult(Result.Success());
            },
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(0, invocationCount);
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_ReturnsValidationFailureNamingTheProperty()
    {
        var behavior = new ValidationBehavior<TestCommand, Result>([NotEmptyValidator()]);

        var result = await behavior.Handle(
            new TestCommand(string.Empty),
            _ => Task.FromResult(Result.Success()),
            CancellationToken.None);

        Assert.Equal(ErrorType.Validation, result.Error?.Type);
        Assert.NotNull(result.Error?.ValidationErrors);
        Assert.True(result.Error!.ValidationErrors!.ContainsKey(nameof(TestCommand.Value)));
    }

    /// <summary>
    /// Proves the generic factory can build a failed <c>Result&lt;T&gt;</c>, not just a
    /// non-generic <c>Result</c> — the reflection path that generic pipeline code depends on.
    /// </summary>
    [Fact]
    public async Task Handle_WithInvalidRequest_ReturnsFailedGenericResult()
    {
        var behavior = new ValidationBehavior<TestCommandWithResponse, Result<string>>(
            [NotEmptyResponseValidator()]);

        var result = await behavior.Handle(
            new TestCommandWithResponse(string.Empty),
            _ => Task.FromResult(Result<string>.Success("handled")),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error?.Type);
        Assert.Null(result.Value);
    }

    private static InlineValidator<TestCommand> NotEmptyValidator()
    {
        var validator = new InlineValidator<TestCommand>();
        validator.RuleFor(command => command.Value).NotEmpty();

        return validator;
    }

    private static InlineValidator<TestCommandWithResponse> NotEmptyResponseValidator()
    {
        var validator = new InlineValidator<TestCommandWithResponse>();
        validator.RuleFor(command => command.Value).NotEmpty();

        return validator;
    }

    private sealed record TestCommand(string Value) : ICommand;

    private sealed record TestCommandWithResponse(string Value) : ICommand<string>;
}
