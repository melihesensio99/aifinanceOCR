# Create solution
dotnet new sln -n AIFinancePlatform

# Create projects
dotnet new classlib -n AIFinancePlatform.Domain -o src/Core/AIFinancePlatform.Domain
dotnet new classlib -n AIFinancePlatform.Application -o src/Core/AIFinancePlatform.Application
dotnet new classlib -n AIFinancePlatform.Persistence -o src/Infrastructure/AIFinancePlatform.Persistence
dotnet new classlib -n AIFinancePlatform.Infrastructure -o src/Infrastructure/AIFinancePlatform.Infrastructure
dotnet new webapi -n AIFinancePlatform.API -o src/Presentation/AIFinancePlatform.API
dotnet new console -n AIFinancePlatform.BackgroundWorker -o src/Presentation/AIFinancePlatform.BackgroundWorker

# Add projects to solution
dotnet sln AIFinancePlatform.sln add src/Core/AIFinancePlatform.Domain/AIFinancePlatform.Domain.csproj
dotnet sln AIFinancePlatform.sln add src/Core/AIFinancePlatform.Application/AIFinancePlatform.Application.csproj
dotnet sln AIFinancePlatform.sln add src/Infrastructure/AIFinancePlatform.Persistence/AIFinancePlatform.Persistence.csproj
dotnet sln AIFinancePlatform.sln add src/Infrastructure/AIFinancePlatform.Infrastructure/AIFinancePlatform.Infrastructure.csproj
dotnet sln AIFinancePlatform.sln add src/Presentation/AIFinancePlatform.API/AIFinancePlatform.API.csproj
dotnet sln AIFinancePlatform.sln add src/Presentation/AIFinancePlatform.BackgroundWorker/AIFinancePlatform.BackgroundWorker.csproj

# Add project references
# Application references Domain
dotnet add src/Core/AIFinancePlatform.Application/AIFinancePlatform.Application.csproj reference src/Core/AIFinancePlatform.Domain/AIFinancePlatform.Domain.csproj

# Persistence references Application and Domain
dotnet add src/Infrastructure/AIFinancePlatform.Persistence/AIFinancePlatform.Persistence.csproj reference src/Core/AIFinancePlatform.Application/AIFinancePlatform.Application.csproj

# Infrastructure references Application and Domain
dotnet add src/Infrastructure/AIFinancePlatform.Infrastructure/AIFinancePlatform.Infrastructure.csproj reference src/Core/AIFinancePlatform.Application/AIFinancePlatform.Application.csproj

# API references Infrastructure and Persistence
dotnet add src/Presentation/AIFinancePlatform.API/AIFinancePlatform.API.csproj reference src/Infrastructure/AIFinancePlatform.Infrastructure/AIFinancePlatform.Infrastructure.csproj
dotnet add src/Presentation/AIFinancePlatform.API/AIFinancePlatform.API.csproj reference src/Infrastructure/AIFinancePlatform.Persistence/AIFinancePlatform.Persistence.csproj

# BackgroundWorker references Infrastructure and Persistence
dotnet add src/Presentation/AIFinancePlatform.BackgroundWorker/AIFinancePlatform.BackgroundWorker.csproj reference src/Infrastructure/AIFinancePlatform.Infrastructure/AIFinancePlatform.Infrastructure.csproj
dotnet add src/Presentation/AIFinancePlatform.BackgroundWorker/AIFinancePlatform.BackgroundWorker.csproj reference src/Infrastructure/AIFinancePlatform.Persistence/AIFinancePlatform.Persistence.csproj

Write-Host "Project structure set up successfully!"
