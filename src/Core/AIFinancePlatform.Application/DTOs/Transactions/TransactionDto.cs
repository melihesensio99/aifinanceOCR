using System;

namespace AIFinancePlatform.Application.DTOs.Transactions;

public class TransactionDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Type { get; init; } = string.Empty;
    public DateTime Date { get; init; }
    public string Description { get; init; } = string.Empty;
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string CategoryIcon { get; init; } = string.Empty;
    public string CategoryColorHex { get; init; } = string.Empty;
    public bool IsAutomatic { get; init; }
    public string? Source { get; init; }
}
