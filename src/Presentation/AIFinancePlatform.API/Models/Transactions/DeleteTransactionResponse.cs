using System;

namespace AIFinancePlatform.API.Models.Transactions;

public record DeleteTransactionResponse(
    Guid DeletedId,
    string Message
);
