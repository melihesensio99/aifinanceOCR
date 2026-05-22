using System;

namespace AIFinancePlatform.Application.DTOs.Transactions;

public record TransactionDto(
    Guid Id,
    string Title,
    decimal Amount,
    string Type,
    DateTime Date,
    string Description,
    Guid CategoryId,
    string CategoryName,
    string CategoryIcon,
    string CategoryColorHex,
    bool IsAutomatic,
    string? Source
);
