using System;

namespace AIFinancePlatform.Domain.Entities;

public class Budget
{
    public Guid Id { get; set; }
    public decimal LimitAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Foreign Keys & Navigation Properties
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid? CategoryId { get; set; } // Nullable if budget is for total spending
    public Category? Category { get; set; }
}
