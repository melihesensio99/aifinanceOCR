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
        _validators = validators ?? Enumerable.Empty<IValidator<TRequest>>();
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            // Not: Select direkt olarak WhenAll'a geçildiğinde state machine'i iki kere tetiklememesi için
            // eğer tekrar iterate edeceksek ToList() faydalıdır, ancak burada tek seferde consume ediliyor.
            var validators = _validators.Select(v => v.ValidateAsync(context, cancellationToken));
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
                            // Not: Burada errorMessages.Distinct() kullanarak property bazlı (lokal) tekilleştirme yapıyoruz. 
                            // Aynı mesaj farklı property'ler için çıkarsa kaybolmaz.
                            Values = errorMessages.Distinct().ToArray()
                        })
                    .ToDictionary(x => x.Key, x => x.Values);

                if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
                {
                    var resultType = typeof(TResponse).GetGenericArguments()[0];
                    var failureMethod = typeof(Result<>)
                        .MakeGenericType(resultType)
                        .GetMethod(nameof(Result<object>.ValidationFailure));
                    
                    return (TResponse)failureMethod!.Invoke(null, new object[] { validationErrors })!;
                }
                else if (typeof(TResponse) == typeof(Result))
                {
                    return (TResponse)(object)Result.ValidationFailure(validationErrors);
                }
                
                // Sistemdeki tüm RequestHandler'lar Result dönmek üzere tasarlanmıştır.
                // Eğer Result dönmeyen bir Handler tanımlanırsa ve hata fırlatırsa, bu bir mimari ihlalidir.
                throw new InvalidOperationException($"MediatR Pipeline Error: {typeof(TRequest).Name} handler did not return a Result or Result<T> object.");
            }
        }

        return await next();
    }
}
