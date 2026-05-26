using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AIFinancePlatform.Application.Common.Interfaces.Persistence;
using AIFinancePlatform.Application.DTOs.Transactions;
using AIFinancePlatform.Domain.Entities;
using AIFinancePlatform.Domain.Enums;

namespace AIFinancePlatform.Application.CQRS.Commands.Transactions.CreateTransaction;

public record CreateTransactionCommand(
    Guid UserId,
    string Title,
    decimal Amount,
    string Type, // "Income" or "Expense"
    DateTime Date,
    string Description,
    Guid CategoryId,
    bool IsAutomatic,
    string? Source,
    string? ReceiptImageUrl
) : IRequest<CreateTransactionCommandResult>;

public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, CreateTransactionCommandResult>
{
    private readonly IApplicationDbContext _context;

    public CreateTransactionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CreateTransactionCommandResult> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<TransactionType>(request.Type, out var transactionType))
        {
            throw new ArgumentException("Geçersiz işlem tipi. 'Income' veya 'Expense' olmalıdır.");
        }

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && (c.IsDefault || c.UserId == request.UserId), cancellationToken);

        if (category == null)
        {
            throw new Exception("Kategori bulunamadı.");
        }

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Title = request.Title,
            Amount = request.Amount,
            Type = transactionType,
            Date = request.Date,
            Description = request.Description,
            CategoryId = request.CategoryId,
            IsAutomatic = request.IsAutomatic,
            Source = string.IsNullOrEmpty(request.Source) ? "Manual" : request.Source,
            ReceiptImageUrl = request.ReceiptImageUrl
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        var transactionDto = new TransactionDto
        {
            Id = transaction.Id,
            Title = transaction.Title,
            Amount = transaction.Amount,
            Type = transaction.Type.ToString(),
            Date = transaction.Date,
            Description = transaction.Description,
            CategoryId = transaction.CategoryId,
            CategoryName = category.Name,
            CategoryIcon = category.Icon,
            CategoryColorHex = category.ColorHex,
            IsAutomatic = transaction.IsAutomatic,
            Source = transaction.Source
        };

        return new CreateTransactionCommandResult(transactionDto, true, "Harcama başarıyla oluşturuldu.");
    }
}
