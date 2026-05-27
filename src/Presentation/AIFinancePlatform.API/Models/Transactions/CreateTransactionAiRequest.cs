using System;

namespace AIFinancePlatform.API.Models.Transactions;

public record CreateTransactionAiRequest(
    Guid UserId,
    string Title,
    decimal Amount,
    string Type,
    DateTime Date,
    string Description,
    Guid CategoryId,
    string? ReceiptImageUrl = null
);
