using System.Collections.Generic;

namespace AIFinancePlatform.Application.Common.Interfaces;

public interface IValidationResult
{
    IDictionary<string, string[]> ValidationErrors { get; }
}
