using System;

namespace AIFinancePlatform.API.Models.Transactions;

public record CreateTransactionRequest(
    string Title,
    decimal Amount,
    string Type,
    DateTime Date,
    string Description,
    Guid CategoryId
);
