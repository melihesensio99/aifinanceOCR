using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using AIFinancePlatform.Application.Common.Models;

namespace AIFinancePlatform.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var validators = _validators.Select(v => v.ValidateAsync(context, cancellationToken)).ToList();
            var validationResults = await Task.WhenAll(validators);

            var failures = validationResults
                .Where(r => r.Errors.Any())
                .SelectMany(r => r.Errors)
                .ToList();

            if (failures.Any())
            {
                var validationErrors = failures
                    .GroupBy(
                        e => e.PropertyName,
                        e => e.ErrorMessage,
                        (propertyName, errorMessages) => new
                        {
                            Key = propertyName,
                            Values = errorMessages.Distinct().ToArray()
                        })
                    .ToDictionary(x => x.Key, x => x.Values);

                if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
                {
                    var resultType = typeof(TResponse).GetGenericArguments()[0];
                    var validationResultType = typeof(ValidationResult<>).MakeGenericType(resultType);
                    
                    var obj = Activator.CreateInstance(
                        validationResultType,
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                        null,
                        new object[] { validationErrors },
                        null);

                    return (TResponse)obj!;
                }
                else if (typeof(TResponse) == typeof(Result))
                {
                    return (TResponse)(object)ValidationResult.WithErrors(validationErrors);
                }
                
                // Eğer TResponse bir Result tipi değilse, eski sistem devam etsin (Güvenlik için)
                throw new Common.Exceptions.ValidationException(failures);
            }
        }

        return await next();
    }
}
