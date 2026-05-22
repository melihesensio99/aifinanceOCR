using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AIFinancePlatform.Application.Common.Interfaces.Persistence;

namespace AIFinancePlatform.Application.CQRS.Commands.Transactions.DeleteTransaction;

public record DeleteTransactionCommand(Guid TransactionId, Guid UserId) : IRequest<DeleteTransactionCommandResult>;

public class DeleteTransactionCommandHandler : IRequestHandler<DeleteTransactionCommand, DeleteTransactionCommandResult>
{
    private readonly IApplicationDbContext _context;

    public DeleteTransactionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DeleteTransactionCommandResult> Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId && t.UserId == request.UserId, cancellationToken);

        if (transaction == null)
        {
            throw new Exception("İşlem bulunamadı veya bu işlemi silme yetkiniz yok.");
        }

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        return new DeleteTransactionCommandResult(request.TransactionId, true, "İşlem başarıyla silindi.");
    }
}
