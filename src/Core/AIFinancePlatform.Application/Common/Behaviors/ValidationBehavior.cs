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

            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .Where(r => r.Errors.Any())
                .SelectMany(r => r.Errors)
                .Select(f => f.ErrorMessage)
                .ToList();

            if (failures.Any())
            {
                if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
                {
                    var resultType = typeof(TResponse).GetGenericArguments()[0];
                    var failureMethod = typeof(Result<>)
                        .MakeGenericType(resultType)
                        .GetMethod("Failure", new[] { typeof(string), typeof(IReadOnlyCollection<string>) });

                    return (TResponse)failureMethod.Invoke(null, new object[] { "Validasyon Hatası", failures });
                }
                else if (typeof(TResponse) == typeof(Result))
                {
                    return (TResponse)(object)Result.Failure("Validasyon Hatası", failures);
                }
                
                // Eğer TResponse bir Result tipi değilse, eski sistem devam etsin (Güvenlik için)
                throw new Common.Exceptions.ValidationException(validationResults.SelectMany(r => r.Errors));
            }
        }

        return await next();
    }
}
