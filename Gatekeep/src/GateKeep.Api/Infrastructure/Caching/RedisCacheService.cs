using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;
using GateKeep.Api.Infrastructure.Observability;
using Microsoft.Extensions.Logging;

namespace GateKeep.Api.Infrastructure.Caching;

/// <summary>
/// Implementación del servicio de cache usando Redis
/// </summary>
public sealed class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer _redis;
    private readonly ICacheMetricsService _metrics;
    private readonly IObservabilityService _observabilityService;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(
        IDistributedCache cache, 
        IConnectionMultiplexer redis,
        ICacheMetricsService metrics,
        IObservabilityService observabilityService,
        ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        _redis = redis;
        _metrics = metrics;
        _observabilityService = observabilityService;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var value = await _cache.GetStringAsync(key);
        
        if (value is null)
        {
            _metrics.RecordMiss(key);
            _observabilityService.RecordCacheOperation("get", false);
            _logger.LogWarning("-----[CACHE]--- cache miss: {Key}", key);
            return null;
        }

        _metrics.RecordHit(key);
        _observabilityService.RecordCacheOperation("get", true);
        _logger.LogWarning("-----[CACHE]--- cache hit: {Key}", key);
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
        _observabilityService.RecordCacheOperation("set", true);
        _logger.LogWarning("-----[CACHE]--- cache set: {Key}, Expiration={Expiration}", key, expiration);
    }

    public async Task RemoveAsync(string key)
    {
        await _cache.RemoveAsync(key);
        _metrics.RecordInvalidation(key);
        _observabilityService.RecordCacheOperation("remove", true);
        _logger.LogWarning("-----[CACHE]--- cache removed: {Key}", key);
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

