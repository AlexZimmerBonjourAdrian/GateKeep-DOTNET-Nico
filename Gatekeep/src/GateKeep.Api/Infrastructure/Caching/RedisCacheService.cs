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
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener del cache Redis para clave {Key}. Continuando sin cache.", key);
            _metrics.RecordMiss(key);
            _observabilityService.RecordCacheOperation("get", false);
            // Fallback: retornar null para que se obtenga de la BD
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar en cache Redis para clave {Key}. Continuando sin cache.", key);
            _observabilityService.RecordCacheOperation("set", false);
            // Fallback: no hacer nada, la operación continúa sin cache
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _cache.RemoveAsync(key);
            _metrics.RecordInvalidation(key);
            _observabilityService.RecordCacheOperation("remove", true);
            _logger.LogWarning("-----[CACHE]--- cache removed: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar del cache Redis para clave {Key}. Continuando sin cache.", key);
            _observabilityService.RecordCacheOperation("remove", false);
            // Fallback: no hacer nada
        }
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: pattern).ToList();
            
            foreach (var key in keys)
            {
                await _cache.RemoveAsync(key.ToString());
                _metrics.RecordInvalidation(key.ToString());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar patrones del cache Redis para patrón {Pattern}. Continuando sin cache.", pattern);
            // Fallback: no hacer nada
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            var value = await _cache.GetStringAsync(key);
            return value is not null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar existencia en cache Redis para clave {Key}. Retornando false.", key);
            return false;
        }
    }
}

