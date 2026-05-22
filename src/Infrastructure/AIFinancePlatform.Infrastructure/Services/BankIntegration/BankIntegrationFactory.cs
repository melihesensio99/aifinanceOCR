using System;
using Microsoft.Extensions.DependencyInjection;
using AIFinancePlatform.Application.Common.Interfaces.BankIntegration;
using AIFinancePlatform.Domain.Enums;

namespace AIFinancePlatform.Infrastructure.Services.BankIntegration;

public class BankIntegrationFactory : IBankIntegrationFactory
{
    private readonly IServiceProvider _serviceProvider;

    public BankIntegrationFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IBankIntegrationService CreateService(BankType bankType)
    {
        return bankType switch
        {
            BankType.Garanti => _serviceProvider.GetRequiredService<MockGarantiBankService>(),
            BankType.Akbank => _serviceProvider.GetRequiredService<MockAkbankService>(),
            BankType.Ziraat => _serviceProvider.GetRequiredService<MockZiraatBankService>(),
            _ => throw new ArgumentException($"Bank type {bankType} is not supported.")
        };
    }
}