using System;
using AIFinancePlatform.Application.DTOs.Transactions;

namespace AIFinancePlatform.Application.CQRS.Commands.Transactions.CreateTransaction;

public record CreateTransactionCommandResult(
    TransactionDto Transaction,
    bool IsSuccess,
    string Message
);
