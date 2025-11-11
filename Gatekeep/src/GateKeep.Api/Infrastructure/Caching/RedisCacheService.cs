using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;

namespace GateKeep.Api.Infrastructure.Caching;

/// <summary>
/// Implementación del servicio de cache usando Redis
/// </summary>
public sealed class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer _redis;
    private readonly ICacheMetricsService _metrics;

    public RedisCacheService(
        IDistributedCache cache, 
        IConnectionMultiplexer redis,
        ICacheMetricsService metrics)
    {
        _cache = cache;
        _redis = redis;
        _metrics = metrics;
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var value = await _cache.GetStringAsync(key);
        
        if (value is null)
        {
            _metrics.RecordMiss(key);
            return null;
        }

        _metrics.RecordHit(key);
        return JsonSerializer.Deserialize<T>(value);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        var serializedValue = JsonSerializer.Serialize(value);
        
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(10)
        };

        await _cache.SetStringAsync(key, serializedValue, options);
    }

    public async Task RemoveAsync(string key)
    {
        await _cache.RemoveAsync(key);
        _metrics.RecordInvalidation(key);
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        var keys = server.Keys(pattern: pattern).ToList();
        
        foreach (var key in keys)
        {
            await _cache.RemoveAsync(key.ToString());
            _metrics.RecordInvalidation(key.ToString());
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        var value = await _cache.GetStringAsync(key);
        return value is not null;
    }
}

