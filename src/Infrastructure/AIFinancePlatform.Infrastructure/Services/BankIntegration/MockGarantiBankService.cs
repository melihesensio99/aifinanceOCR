using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AIFinancePlatform.Application.Common.Interfaces.BankIntegration;
using AIFinancePlatform.Application.DTOs.Transactions;

namespace AIFinancePlatform.Infrastructure.Services.BankIntegration;

public class MockGarantiBankService : IBankIntegrationService
{
    public Task<List<BankTransactionDto>> FetchTransactionsAsync(string accountId)
    {
        // Garanti Bankası'na bağlanıyormuş gibi sahte veriler dönelim
        return Task.FromResult(new List<BankTransactionDto>
        {
            new BankTransactionDto
            {
                Title = "Migros Market",
                Amount = 150.75m,
                Date = DateTime.UtcNow.AddDays(-1),
                Description = "Gıda Alışverişi"
            },
            new BankTransactionDto
            {
                Title = "Netflix",
                Amount = 149.99m,
                Date = DateTime.UtcNow.AddDays(-2),
                Description = "Aylık Abonelik"
            }
        });
    }
}