using FluentValidation;
using MediatR;
using FleetOS.Shared.Results;

namespace FleetOS.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that automatically runs all FluentValidation validators
/// for the incoming request before passing it to the handler.
/// If validation fails, it returns a failed Result containing the first error.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : class
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .Where(r => r.Errors.Count > 0)
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Count > 0)
        {
            var firstFailure = failures.First();
            var error = Error.Validation(firstFailure.PropertyName, firstFailure.ErrorMessage);
            
            // Assuming TResponse is a Result or Result<T>
            var responseType = typeof(TResponse);

            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var resultType = responseType.GetGenericArguments()[0];
                var failureMethod = typeof(Result)
                    .GetMethods()
                    .First(m => m.Name == "Failure" && m.IsGenericMethod)
                    .MakeGenericMethod(resultType);

                return (TResponse)failureMethod.Invoke(null, [error])!;
            }

            if (responseType == typeof(Result))
            {
                return (TResponse)(object)Result.Failure(error);
            }

            throw new ValidationException(failures);
        }

        return await next();
    }
}
