using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AIFinancePlatform.Application.Common.Interfaces.Persistence;
using AIFinancePlatform.Application.DTOs.Transactions;

using AIFinancePlatform.Application.Common.Models;
using AIFinancePlatform.Application.Common.Mappings;

namespace AIFinancePlatform.Application.CQRS.Queries.Transactions.GetTransactions;

public record GetTransactionsQuery(Guid UserId, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<TransactionDto>>;

public class GetTransactionsQueryHandler : IRequestHandler<GetTransactionsQuery, PaginatedList<TransactionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTransactionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<TransactionDto>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Transactions
            .Where(t => t.UserId == request.UserId)
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                Title = t.Title,
                Amount = t.Amount,
                Type = t.Type.ToString(),
                Date = t.Date,
                Description = t.Description,
                CategoryId = t.CategoryId,
                CategoryName = t.Category != null ? t.Category.Name : string.Empty,
                CategoryIcon = t.Category != null ? t.Category.Icon : string.Empty,
                CategoryColorHex = t.Category != null ? t.Category.ColorHex : string.Empty,
                IsAutomatic = t.IsAutomatic,
                Source = t.Source
            })
            .OrderByDescending(t => t.Date)
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
