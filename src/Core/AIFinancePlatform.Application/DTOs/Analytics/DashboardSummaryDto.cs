using System.Collections.Generic;

namespace AIFinancePlatform.Application.DTOs.Analytics;

public class DashboardSummaryDto
{
    public decimal TotalExpense { get; set; }
    public int TotalTransactionCount { get; set; }
    public List<CategorySummaryDto> CategoryBreakdown { get; set; } = new();
}
