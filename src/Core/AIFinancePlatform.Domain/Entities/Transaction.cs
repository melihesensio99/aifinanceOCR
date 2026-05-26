using System;
using AIFinancePlatform.Domain.Enums;

namespace AIFinancePlatform.Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    
    // Foreign Keys & Navigation Properties
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public bool IsAutomatic { get; set; } = false;
    public string? Source { get; set; } // "Manual", "OCR", "MockBank"
    public string? ReceiptImageUrl { get; set; } // Orijinal fişin bulut veya lokal linki
}
