using System;
using AIFinancePlatform.Domain.Enums;

namespace AIFinancePlatform.Domain.Entities;

public class Receipt
{
    public Guid Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public ReceiptStatus Status { get; set; } = ReceiptStatus.Uploaded;
    public string? RawOcrText { get; set; }
    public string? ParsedJson { get; set; } // OpenAI JSON response
    
    // Foreign Keys & Navigation Properties
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
