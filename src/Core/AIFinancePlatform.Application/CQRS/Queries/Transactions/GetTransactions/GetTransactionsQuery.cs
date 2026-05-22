using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AIFinancePlatform.Application.Common.Interfaces.Persistence;
using AIFinancePlatform.Application.DTOs.Transactions;

namespace AIFinancePlatform.Application.CQRS.Queries.Transactions.GetTransactions;

public record GetTransactionsQuery(Guid UserId) : IRequest<List<TransactionDto>>;

public class GetTransactionsQueryHandler : IRequestHandler<GetTransactionsQuery, List<TransactionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTransactionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TransactionDto>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == request.UserId)
            .OrderByDescending(t => t.Date)
            .Select(t => new TransactionDto(
                t.Id,
                t.Title,
                t.Amount,
                t.Type.ToString(),
                t.Date,
                t.Description,
                t.CategoryId,
                t.Category.Name,
                t.Category.Icon,
                t.Category.ColorHex,
                t.IsAutomatic,
                t.Source
            ))
            .ToListAsync(cancellationToken);
    }
}
