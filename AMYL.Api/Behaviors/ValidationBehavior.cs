using AMYL.Api.Abstractions.Messaging;
using AMYL.Api.Domain.Common;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace AMYL.Api.Behaviors;

/// <summary>
/// Validates any message whose response is a <see cref="Result"/> and returns a failed
/// <see cref="Result"/> — it never throws. A validation failure is an expected outcome,
/// so it travels the Result path; only unexpected faults reach <c>GlobalExceptionHandler</c>.
/// </summary>
/// <remarks>
/// <para>
/// <c>TResponse.ValidationFailure(...)</c> is a static abstract interface member, so the
/// correct factory — <see cref="Result"/>'s or <see cref="Result{T}"/>'s — is chosen at
/// compile time. No reflection, and a response type that forgets to implement
/// <see cref="IValidationResult{TSelf}"/> is a build error rather than a runtime one.
/// </para>
/// <para>
/// This deliberately validates <b>queries as well as commands</b>, because several list
/// slices validate their pagination bounds. Constraining to <see cref="IBaseCommand"/>
/// would silently stop running those validators.
/// </para>
/// <para>
/// If you later add a behaviour that must apply to writes only — a transaction or outbox
/// dispatch — constrain it on the non-generic <see cref="IBaseCommand"/> marker. Never on
/// <c>ICommand&lt;TResponse&gt;</c>: MediatR closes a behaviour with TResponse =
/// <c>Result&lt;T&gt;</c>, so that constraint demands <c>ICommand&lt;Result&lt;T&gt;&gt;</c>,
/// never matches, and disables the behaviour with no error.
/// </para>
/// </remarks>
public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result, IValidationResult<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var validatorArray = validators as IValidator<TRequest>[] ?? validators.ToArray();

        if (validatorArray.Length == 0)
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);

        ValidationResult[] results = await Task.WhenAll(
            validatorArray.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        Dictionary<string, string[]> errors = results
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .GroupBy(failure => failure.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).Distinct().ToArray());

        return errors.Count == 0
            ? await next(cancellationToken)
            : TResponse.ValidationFailure(errors);
    }
}
