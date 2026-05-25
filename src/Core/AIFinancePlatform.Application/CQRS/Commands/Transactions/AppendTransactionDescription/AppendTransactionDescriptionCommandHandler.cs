using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AIFinancePlatform.Application.Common.Interfaces.Persistence;

namespace AIFinancePlatform.Application.CQRS.Commands.Transactions.AppendTransactionDescription;

public class AppendTransactionDescriptionCommandHandler : IRequestHandler<AppendTransactionDescriptionCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public AppendTransactionDescriptionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(AppendTransactionDescriptionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _context.Transactions.FindAsync(new object[] { request.Id }, cancellationToken);
        if (transaction == null) return false;

        transaction.Description += $"\n{request.TextToAppend}";
        await _context.SaveChangesAsync(cancellationToken);
        
        return true;
    }
}
