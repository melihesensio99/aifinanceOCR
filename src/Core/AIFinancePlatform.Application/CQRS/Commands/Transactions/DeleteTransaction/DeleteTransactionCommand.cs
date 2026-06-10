using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AIFinancePlatform.Application.Common.Interfaces.Persistence;

using AIFinancePlatform.Application.Common.Models;

namespace AIFinancePlatform.Application.CQRS.Commands.Transactions.DeleteTransaction;

public record DeleteTransactionCommand(Guid TransactionId, Guid UserId) : IRequest<Result<Guid>>;

public class DeleteTransactionCommandHandler : IRequestHandler<DeleteTransactionCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public DeleteTransactionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId && t.UserId == request.UserId, cancellationToken);

        if (transaction == null)
        {
            return Result<Guid>.Failure("İşlem bulunamadı veya bu işlemi silme yetkiniz yok.");
        }

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(request.TransactionId);
    }
}
