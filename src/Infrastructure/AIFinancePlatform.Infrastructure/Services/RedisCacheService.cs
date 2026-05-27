using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using AIFinancePlatform.Application.Common.Interfaces.Services;

namespace AIFinancePlatform.Infrastructure.Services;

public class RedisCacheService : IRedisCacheService
{
    private readonly IDatabase _db;

    public RedisCacheService(IConfiguration configuration)
    {
        var connectionString = configuration["Redis:ConnectionString"] ?? "localhost:6379";
        var connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);
        _db = connectionMultiplexer.GetDatabase();
    }

    public async Task<string?> GetCacheValueAsync(string key)
    {
        var value = await _db.StringGetAsync(key);
        return value.HasValue ? value.ToString() : null;
    }

    public async Task SetCacheValueAsync(string key, string value, TimeSpan? expirationTime = null)
    {
        var expiry = expirationTime ?? TimeSpan.FromDays(7); // Varsayılan 7 gün
        await _db.StringSetAsync(key, value, expiry);
    }

    public async Task RemoveCacheValueAsync(string key)
    {
        await _db.KeyDeleteAsync(key);
    }
}
