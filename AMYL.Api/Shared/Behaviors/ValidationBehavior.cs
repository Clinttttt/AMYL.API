using FluentValidation;
using MediatR;

namespace AMYL.Api.Shared.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var validatorArray = validators as IValidator<TRequest>[] ?? validators.ToArray();

        if (validatorArray.Length == 0)
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            validatorArray.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        var errors = validationResults
            .SelectMany(result => result.Errors)
            .Where(error => error is not null)
            .ToArray();

        if (errors.Length > 0)
        {
            throw new ValidationException(errors);
        }

        return await next();
    }
}
