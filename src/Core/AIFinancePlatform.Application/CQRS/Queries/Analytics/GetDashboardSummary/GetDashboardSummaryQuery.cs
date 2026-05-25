using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AIFinancePlatform.Application.Common.Interfaces.Persistence;
using AIFinancePlatform.Application.DTOs.Analytics;
using AIFinancePlatform.Domain.Enums;

namespace AIFinancePlatform.Application.CQRS.Queries.Analytics.GetDashboardSummary;

public record GetDashboardSummaryQuery(Guid UserId, TimePeriod Period = TimePeriod.AllTime) : IRequest<DashboardSummaryDto>;

public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private readonly IApplicationDbContext _context;

    public GetDashboardSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == request.UserId);

        var now = DateTime.UtcNow;
        var startDate = DateTime.MinValue;
        var endDate = now;

        if (request.Period == TimePeriod.Weekly)
        {
            startDate = now.AddDays(-7);
            query = query.Where(t => t.Date >= startDate);
        }
        else if (request.Period == TimePeriod.Monthly)
        {
            startDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            query = query.Where(t => t.Date >= startDate);
        }
        else if (request.Period == TimePeriod.Yearly)
        {
            startDate = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            query = query.Where(t => t.Date >= startDate);
        }

        var transactions = await query.ToListAsync(cancellationToken);

        var totalExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        var totalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var netBalance = totalIncome - totalExpense;
        var totalCount = transactions.Count;

        var categorySummaries = transactions
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

        return new DashboardSummaryDto
        {
            TotalExpense = totalExpense,
            TotalIncome = totalIncome,
            NetBalance = netBalance,
            TransactionCount = totalCount,
            StartDate = startDate,
            EndDate = endDate,
            CategorySummaries = categorySummaries
        };
    }
}
