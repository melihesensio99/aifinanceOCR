using System.Collections.Generic;
using System.Threading.Tasks;
using AIFinancePlatform.Application.DTOs.Transactions;

namespace AIFinancePlatform.Application.Common.Interfaces.BankIntegration;

public interface IBankIntegrationService
{
    Task<List<BankTransactionDto>> FetchTransactionsAsync(string accountId);
}