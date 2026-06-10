using System.Collections.Generic;
using AIFinancePlatform.Application.Common.Interfaces;

namespace AIFinancePlatform.Application.Common.Models;

public sealed class ValidationResult : Result, IValidationResult
{
    public IDictionary<string, string[]> ValidationErrors { get; }

    internal ValidationResult(IDictionary<string, string[]> validationErrors) 
        : base(false, "Validasyon Hatası", null)
    {
        ValidationErrors = validationErrors;
    }

    public static ValidationResult WithErrors(IDictionary<string, string[]> validationErrors) => new(validationErrors);
}

public sealed class ValidationResult<TValue> : Result<TValue>, IValidationResult
{
    public IDictionary<string, string[]> ValidationErrors { get; }

    internal ValidationResult(IDictionary<string, string[]> validationErrors) 
        : base(false, "Validasyon Hatası", default, null)
    {
        ValidationErrors = validationErrors;
    }

    public static ValidationResult<TValue> WithErrors(IDictionary<string, string[]> validationErrors) => new(validationErrors);
}
