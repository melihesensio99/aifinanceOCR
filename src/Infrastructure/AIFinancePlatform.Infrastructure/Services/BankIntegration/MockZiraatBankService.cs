using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AIFinancePlatform.Application.Common.Interfaces.BankIntegration;
using AIFinancePlatform.Application.DTOs.Transactions;

namespace AIFinancePlatform.Infrastructure.Services.BankIntegration;

public class MockZiraatBankService : IBankIntegrationService
{
    public Task<List<BankTransactionDto>> FetchTransactionsAsync(string accountId)
    {
        // Ziraat Bankası'na bağlanıyormuş gibi sahte veriler dönelim
        return Task.FromResult(new List<BankTransactionDto>
        {
            new BankTransactionDto
            {
                Title = "KYK Kredi Ödemesi",
                Amount = 2000.00m,
                Date = DateTime.UtcNow.AddDays(-5),
                Description = "Kredi Ödemesi"
            },
            new BankTransactionDto
            {
                Title = "A101 Market",
                Amount = 340.25m,
                Date = DateTime.UtcNow.AddDays(-1),
                Description = "Market Alışverişi"
            }
        });
    }
}