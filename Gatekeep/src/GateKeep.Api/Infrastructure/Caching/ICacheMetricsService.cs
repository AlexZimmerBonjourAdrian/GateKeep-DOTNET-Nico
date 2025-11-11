namespace GateKeep.Api.Infrastructure.Caching;

/// <summary>
/// Servicio para registrar métricas del cache (hits, misses, invalidaciones)
/// </summary>
public interface ICacheMetricsService
{
    /// <summary>
    /// Registra un cache hit
    /// </summary>
    void RecordHit(string key);
    
    /// <summary>
    /// Registra un cache miss
    /// </summary>
    void RecordMiss(string key);
    
    /// <summary>
    /// Registra una invalidación de cache
    /// </summary>
    void RecordInvalidation(string key);
    
    /// <summary>
    /// Obtiene las métricas actuales
    /// </summary>
    CacheMetrics GetMetrics();
    
    /// <summary>
    /// Resetea las métricas
    /// </summary>
    void ResetMetrics();
}

/// <summary>
/// Modelo de métricas del cache
/// </summary>
public sealed class CacheMetrics
{
    public long TotalHits { get; set; }
    public long TotalMisses { get; set; }
    public long TotalInvalidations { get; set; }
    public double HitRate => TotalRequests > 0 ? (double)TotalHits / TotalRequests * 100 : 0;
    public long TotalRequests => TotalHits + TotalMisses;
    public Dictionary<string, long> HitsByKey { get; set; } = new();
    public Dictionary<string, long> MissesByKey { get; set; } = new();
    public DateTime LastResetTime { get; set; } = DateTime.UtcNow;
}

