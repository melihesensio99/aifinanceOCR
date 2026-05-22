using System;

namespace AIFinancePlatform.Domain.Entities;

public class Subscription
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int BillingCycleDays { get; set; } = 30; // default to monthly
    public DateTime NextPaymentDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Keys & Navigation Properties
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
