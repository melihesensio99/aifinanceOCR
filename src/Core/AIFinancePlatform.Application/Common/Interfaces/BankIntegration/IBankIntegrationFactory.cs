using AIFinancePlatform.Domain.Enums;

namespace AIFinancePlatform.Application.Common.Interfaces.BankIntegration;

public interface IBankIntegrationFactory
{
    IBankIntegrationService CreateService(BankType bankType);
}