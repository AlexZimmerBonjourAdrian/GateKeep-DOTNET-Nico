namespace GateKeep.Api.Infrastructure.Caching;

/// <summary>
/// Servicio de cache genérico para Redis
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Obtiene un valor del cache
    /// </summary>
    Task<T?> GetAsync<T>(string key) where T : class;
    
    /// <summary>
    /// Establece un valor en el cache con tiempo de expiración
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
    
    /// <summary>
    /// Elimina un valor del cache (invalidación)
    /// </summary>
    Task RemoveAsync(string key);
    
    /// <summary>
    /// Elimina múltiples valores del cache por patrón
    /// </summary>
    Task RemoveByPatternAsync(string pattern);
    
    /// <summary>
    /// Verifica si existe una clave en el cache
    /// </summary>
    Task<bool> ExistsAsync(string key);
}

