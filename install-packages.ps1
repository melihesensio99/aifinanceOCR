# Application packages
dotnet add src/Core/AIFinancePlatform.Application/AIFinancePlatform.Application.csproj package MediatR
dotnet add src/Core/AIFinancePlatform.Application/AIFinancePlatform.Application.csproj package FluentValidation.DependencyInjectionExtensions
dotnet add src/Core/AIFinancePlatform.Application/AIFinancePlatform.Application.csproj package Microsoft.Extensions.DependencyInjection.Abstractions

# Persistence packages
dotnet add src/Infrastructure/AIFinancePlatform.Persistence/AIFinancePlatform.Persistence.csproj package Microsoft.EntityFrameworkCore
dotnet add src/Infrastructure/AIFinancePlatform.Persistence/AIFinancePlatform.Persistence.csproj package Microsoft.EntityFrameworkCore.Design
dotnet add src/Infrastructure/AIFinancePlatform.Persistence/AIFinancePlatform.Persistence.csproj package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/Infrastructure/AIFinancePlatform.Persistence/AIFinancePlatform.Persistence.csproj package Microsoft.EntityFrameworkCore.Tools

# Infrastructure packages
dotnet add src/Infrastructure/AIFinancePlatform.Infrastructure/AIFinancePlatform.Infrastructure.csproj package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add src/Infrastructure/AIFinancePlatform.Infrastructure/AIFinancePlatform.Infrastructure.csproj package System.IdentityModel.Tokens.Jwt
dotnet add src/Infrastructure/AIFinancePlatform.Infrastructure/AIFinancePlatform.Infrastructure.csproj package MassTransit.RabbitMQ
dotnet add src/Infrastructure/AIFinancePlatform.Infrastructure/AIFinancePlatform.Infrastructure.csproj package Microsoft.Extensions.Configuration.Abstractions

# API packages
dotnet add src/Presentation/AIFinancePlatform.API/AIFinancePlatform.API.csproj package Microsoft.EntityFrameworkCore.Design

# BackgroundWorker packages
dotnet add src/Presentation/AIFinancePlatform.BackgroundWorker/AIFinancePlatform.BackgroundWorker.csproj package MassTransit.RabbitMQ
dotnet add src/Presentation/AIFinancePlatform.BackgroundWorker/AIFinancePlatform.BackgroundWorker.csproj package Microsoft.Extensions.Hosting

Write-Host "NuGet packages installed successfully!"
