using System;
using System.Collections.Generic;

namespace AIFinancePlatform.Domain.Entities;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string ColorHex { get; set; } = string.Empty;
    public bool IsDefault { get; set; } = false;
    public Guid? UserId { get; set; }
    
    // Navigation Properties
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
}
