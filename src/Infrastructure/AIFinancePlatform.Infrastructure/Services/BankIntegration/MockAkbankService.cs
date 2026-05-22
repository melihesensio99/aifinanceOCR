using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AIFinancePlatform.Application.Common.Interfaces.BankIntegration;
using AIFinancePlatform.Application.DTOs.Transactions;

namespace AIFinancePlatform.Infrastructure.Services.BankIntegration;

public class MockAkbankService : IBankIntegrationService
{
    public Task<List<BankTransactionDto>> FetchTransactionsAsync(string accountId)
    {
        // Akbank'a bağlanıyormuş gibi sahte veriler dönelim
        return Task.FromResult(new List<BankTransactionDto>
        {
            new BankTransactionDto
            {
                Title = "Starbucks",
                Amount = 120.50m,
                Date = DateTime.UtcNow.AddDays(-1),
                Description = "Kahve"
            },
            new BankTransactionDto
            {
                Title = "Steam",
                Amount = 599.00m,
                Date = DateTime.UtcNow.AddDays(-3),
                Description = "Oyun Harcaması"
            }
        });
    }
}