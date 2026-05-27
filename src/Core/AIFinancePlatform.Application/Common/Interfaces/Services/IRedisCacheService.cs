using System;
using System.Threading.Tasks;

namespace AIFinancePlatform.Application.Common.Interfaces.Services;

public interface IRedisCacheService
{
    Task<string?> GetCacheValueAsync(string key);
    Task SetCacheValueAsync(string key, string value, TimeSpan? expirationTime = null);
    Task RemoveCacheValueAsync(string key);
}
