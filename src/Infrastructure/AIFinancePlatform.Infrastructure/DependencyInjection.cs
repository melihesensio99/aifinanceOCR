using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AIFinancePlatform.Application.Common.Interfaces.Authentication;
using AIFinancePlatform.Infrastructure.Authentication;
using AIFinancePlatform.Application.Common.Interfaces.Events;
using AIFinancePlatform.Infrastructure.Messaging;
using AIFinancePlatform.Application.Common.Interfaces.Services;
using AIFinancePlatform.Infrastructure.Services;
using AIFinancePlatform.Application.Common.Interfaces.BankIntegration;
using AIFinancePlatform.Infrastructure.Services.BankIntegration;

namespace AIFinancePlatform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind JwtSettings config
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        
        // Register Services
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
        services.AddSingleton<IRedisCacheService, RedisCacheService>();

        var storageProvider = configuration["FileStorage:Provider"];
        if (storageProvider == "AWS")
        {
            services.AddScoped<IFileStorageService, AwsS3StorageService>();
        }
        else if (storageProvider == "Azure")
        {
            services.AddScoped<IFileStorageService, AzureBlobStorageService>();
        }
        else
        {
            services.AddScoped<IFileStorageService, LocalFileStorageService>();
        }

        // Register Bank Integration Services
        services.AddScoped<MockGarantiBankService>();
        services.AddScoped<MockAkbankService>();
        services.AddScoped<MockZiraatBankService>();
        services.AddScoped<IBankIntegrationFactory, BankIntegrationFactory>();

        return services;
    }
}
