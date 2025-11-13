using System.Collections.Concurrent;

namespace GateKeep.Api.Infrastructure.Caching;

/// <summary>
/// Implementación del servicio de métricas del cache
/// </summary>
public sealed class CacheMetricsService : ICacheMetricsService
{
    private long _totalHits;
    private long _totalMisses;
    private long _totalInvalidations;
    private readonly ConcurrentDictionary<string, long> _hitsByKey = new();
    private readonly ConcurrentDictionary<string, long> _missesByKey = new();
    private DateTime _lastResetTime = DateTime.UtcNow;

    public void RecordHit(string key)
    {
        Interlocked.Increment(ref _totalHits);
        _hitsByKey.AddOrUpdate(key, 1, (_, count) => count + 1);
    }

    public void RecordMiss(string key)
    {
        Interlocked.Increment(ref _totalMisses);
        _missesByKey.AddOrUpdate(key, 1, (_, count) => count + 1);
    }

    public void RecordInvalidation(string key)
    {
        Interlocked.Increment(ref _totalInvalidations);
    }

    public CacheMetrics GetMetrics()
    {
        return new CacheMetrics
        {
            TotalHits = _totalHits,
            TotalMisses = _totalMisses,
            TotalInvalidations = _totalInvalidations,
            HitsByKey = new Dictionary<string, long>(_hitsByKey),
            MissesByKey = new Dictionary<string, long>(_missesByKey),
            LastResetTime = _lastResetTime
        };
    }

    public void ResetMetrics()
    {
        _totalHits = 0;
        _totalMisses = 0;
        _totalInvalidations = 0;
        _hitsByKey.Clear();
        _missesByKey.Clear();
        _lastResetTime = DateTime.UtcNow;
    }
}

