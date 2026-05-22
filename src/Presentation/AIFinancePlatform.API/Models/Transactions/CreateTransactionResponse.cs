using AIFinancePlatform.Application.DTOs.Transactions;

namespace AIFinancePlatform.API.Models.Transactions;

public record CreateTransactionResponse(
    TransactionDto Transaction,
    string Message
);
