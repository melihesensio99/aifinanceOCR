using System;
using System.Collections.Generic;

namespace AIFinancePlatform.Application.DTOs.Analytics;

public class DashboardSummaryDto
{
    public decimal TotalExpense { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal NetBalance { get; set; }
    public int TransactionCount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<CategorySummaryDto> CategorySummaries { get; set; } = new();
}
