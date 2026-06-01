FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files and restore as distinct layers
COPY ["src/Presentation/AIFinancePlatform.API/AIFinancePlatform.API.csproj", "src/Presentation/AIFinancePlatform.API/"]
COPY ["src/Core/AIFinancePlatform.Application/AIFinancePlatform.Application.csproj", "src/Core/AIFinancePlatform.Application/"]
COPY ["src/Core/AIFinancePlatform.Domain/AIFinancePlatform.Domain.csproj", "src/Core/AIFinancePlatform.Domain/"]
COPY ["src/Infrastructure/AIFinancePlatform.Infrastructure/AIFinancePlatform.Infrastructure.csproj", "src/Infrastructure/AIFinancePlatform.Infrastructure/"]
COPY ["src/Infrastructure/AIFinancePlatform.Persistence/AIFinancePlatform.Persistence.csproj", "src/Infrastructure/AIFinancePlatform.Persistence/"]
RUN dotnet restore "src/Presentation/AIFinancePlatform.API/AIFinancePlatform.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/Presentation/AIFinancePlatform.API"
RUN dotnet build "AIFinancePlatform.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AIFinancePlatform.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AIFinancePlatform.API.dll"]
