using AMYL.Api.Shared.Behaviors;
using FluentValidation;
using MediatR;

namespace AMYL.Tests;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WithoutValidators_InvokesHandlerOnce()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([]);
        var invocationCount = 0;

        var result = await behavior.Handle(
            new TestRequest("valid"),
            _ =>
            {
                invocationCount++;
                return Task.FromResult("handled");
            },
            CancellationToken.None);

        Assert.Equal("handled", result);
        Assert.Equal(1, invocationCount);
    }

    [Fact]
    public async Task Handle_WithValidRequest_InvokesHandlerOnce()
    {
        var validator = new InlineValidator<TestRequest>();
        validator.RuleFor(request => request.Value).NotEmpty();
        var behavior = new ValidationBehavior<TestRequest, string>([validator]);
        var invocationCount = 0;

        await behavior.Handle(
            new TestRequest("valid"),
            _ =>
            {
                invocationCount++;
                return Task.FromResult("handled");
            },
            CancellationToken.None);

        Assert.Equal(1, invocationCount);
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_DoesNotInvokeHandler()
    {
        var validator = new InlineValidator<TestRequest>();
        validator.RuleFor(request => request.Value).NotEmpty();
        var behavior = new ValidationBehavior<TestRequest, string>([validator]);
        var invocationCount = 0;

        await Assert.ThrowsAsync<ValidationException>(() => behavior.Handle(
            new TestRequest(string.Empty),
            _ =>
            {
                invocationCount++;
                return Task.FromResult("handled");
            },
            CancellationToken.None));

        Assert.Equal(0, invocationCount);
    }

    private sealed record TestRequest(string Value) : IRequest<string>;
}
