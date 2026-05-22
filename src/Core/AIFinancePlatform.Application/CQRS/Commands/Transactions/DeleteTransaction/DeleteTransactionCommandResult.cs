using System;

namespace AIFinancePlatform.Application.CQRS.Commands.Transactions.DeleteTransaction;

public record DeleteTransactionCommandResult(
    Guid DeletedId,
    bool IsSuccess,
    string Message
);
