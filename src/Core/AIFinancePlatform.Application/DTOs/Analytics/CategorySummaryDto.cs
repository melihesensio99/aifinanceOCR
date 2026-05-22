using System;

namespace AIFinancePlatform.Application.DTOs.Analytics;

public class CategorySummaryDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryColorHex { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal Percentage { get; set; }
}
