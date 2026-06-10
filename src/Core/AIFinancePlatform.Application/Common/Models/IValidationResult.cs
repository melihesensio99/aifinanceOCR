using System.Collections.Generic;

namespace AIFinancePlatform.Application.Common.Models;

public interface IValidationResult
{
    IDictionary<string, string[]> ValidationErrors { get; }
}
