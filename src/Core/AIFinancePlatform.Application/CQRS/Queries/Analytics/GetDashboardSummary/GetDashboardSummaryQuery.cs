using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AIFinancePlatform.Application.Common.Interfaces.Persistence;
using AIFinancePlatform.Application.DTOs.Analytics;
using AIFinancePlatform.Domain.Enums;
using AIFinancePlatform.Domain.Entities;

using AIFinancePlatform.Application.Common.Models;

namespace AIFinancePlatform.Application.CQRS.Queries.Analytics.GetDashboardSummary;

public record GetDashboardSummaryQuery(Guid UserId, TimePeriod Period = TimePeriod.AllTime) : IRequest<Result<DashboardSummaryDto>>;

public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, Result<DashboardSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDashboardSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<DashboardSummaryDto>> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var (startDate, endDate) = GetDateRangeForPeriod(request.Period);
        
        var query = _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == request.UserId);

        if (startDate != DateTime.MinValue)
        {
            query = query.Where(t => t.Date >= startDate);
        }

        var transactions = await query.ToListAsync(cancellationToken);

        var totalExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        var totalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var netBalance = totalIncome - totalExpense;

        var categorySummaries = CalculateCategorySummaries(transactions, totalExpense);

        return Result<DashboardSummaryDto>.Success(new DashboardSummaryDto
        {
            TotalExpense = totalExpense,
            TotalIncome = totalIncome,
            NetBalance = netBalance,
            TransactionCount = transactions.Count,
            StartDate = startDate,
            EndDate = endDate,
            CategorySummaries = categorySummaries
        });
    }

    private static (DateTime StartDate, DateTime EndDate) GetDateRangeForPeriod(TimePeriod period)
    {
        var now = DateTime.UtcNow;
        var endDate = now;
        
        DateTime startDate = period switch
        {
            TimePeriod.Weekly => now.AddDays(-7),
            TimePeriod.Monthly => new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc),
            TimePeriod.Yearly => new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            _ => DateTime.MinValue
        };

        return (startDate, endDate);
    }

    private static List<CategorySummaryDto> CalculateCategorySummaries(List<Transaction> transactions, decimal totalExpense)
    {
        return transactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.CategoryId)
            .Select(g =>
            {
                var category = g.First().Category;
                var amount = g.Sum(t => t.Amount);
                return new CategorySummaryDto
                {
                    CategoryId = g.Key,
                    CategoryName = category?.Name ?? "Unknown",
                    CategoryIcon = category?.Icon ?? "📌",
                    CategoryColorHex = category?.ColorHex ?? "#808080",
                    TotalAmount = amount,
                    Percentage = totalExpense > 0 ? (amount / totalExpense) * 100 : 0
                };
            })
            .OrderByDescending(c => c.TotalAmount)
            .ToList();
    }
}
