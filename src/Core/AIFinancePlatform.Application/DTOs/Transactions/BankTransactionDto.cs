namespace AIFinancePlatform.Application.DTOs.Transactions;

public class BankTransactionDto
{
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
}