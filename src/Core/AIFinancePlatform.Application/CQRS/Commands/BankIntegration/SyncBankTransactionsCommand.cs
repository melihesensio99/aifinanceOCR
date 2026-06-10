using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AIFinancePlatform.Application.Common.Interfaces.BankIntegration;
using AIFinancePlatform.Application.Common.Interfaces.Persistence;
using AIFinancePlatform.Domain.Enums;
using AIFinancePlatform.Domain.Entities;

using AIFinancePlatform.Application.Common.Models;

namespace AIFinancePlatform.Application.CQRS.Commands.BankIntegration;

public record SyncBankTransactionsCommand(BankType BankType, Guid UserId) : IRequest<Result>;

public class SyncBankTransactionsCommandHandler : IRequestHandler<SyncBankTransactionsCommand, Result>
{
    private readonly IBankIntegrationFactory _bankFactory;
    private readonly IApplicationDbContext _context;

    public SyncBankTransactionsCommandHandler(IBankIntegrationFactory bankFactory, IApplicationDbContext context)
    {
        _bankFactory = bankFactory;
        _context = context;
    }

    public async Task<Result> Handle(SyncBankTransactionsCommand request, CancellationToken cancellationToken)
    {
        var service = _bankFactory.CreateService(request.BankType);
        
        // Use a dummy account ID for mock services
        var dtos = await service.FetchTransactionsAsync("mock-account-123");

        // Find a default category or handle it properly (fallback to a specific one if needed, we'll pick first for simplicity)
        var defaultCategory = _context.Categories.FirstOrDefault(c => c.UserId == request.UserId) 
            ?? _context.Categories.FirstOrDefault();

        var categoryId = defaultCategory?.Id ?? Guid.Parse("11111111-1111-1111-1111-111111111111");

        foreach (var dto in dtos)
        {
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Amount = dto.Amount,
                Date = dto.Date,
                Description = dto.Description,
                Type = TransactionType.Expense, // default
                UserId = request.UserId,
                CategoryId = categoryId,
                IsAutomatic = true,
                Source = "MockBank"
            };

            _context.Transactions.Add(transaction);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}