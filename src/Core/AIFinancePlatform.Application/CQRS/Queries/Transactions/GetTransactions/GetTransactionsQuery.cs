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
            .Select(t => new TransactionDto(
                t.Id,
                t.Title,
                t.Amount,
                t.Type.ToString(),
                t.Date,
                t.Description,
                t.CategoryId,
                t.Category != null ? t.Category.Name : string.Empty,
                t.Category != null ? t.Category.Icon : string.Empty,
                t.Category != null ? t.Category.ColorHex : string.Empty,
                t.IsAutomatic,
                t.Source
            ))
            .OrderByDescending(t => t.Date)
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
