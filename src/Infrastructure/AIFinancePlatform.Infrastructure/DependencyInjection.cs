using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AIFinancePlatform.Application.Common.Interfaces.Authentication;
using AIFinancePlatform.Infrastructure.Authentication;
using AIFinancePlatform.Application.Common.Interfaces.Events;
using AIFinancePlatform.Infrastructure.Messaging;
using AIFinancePlatform.Application.Common.Interfaces.Services;
using AIFinancePlatform.Infrastructure.Services;

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
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        return services;
    }
}
