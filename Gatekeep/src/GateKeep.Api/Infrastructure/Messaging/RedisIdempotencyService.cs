using StackExchange.Redis;

namespace GateKeep.Api.Infrastructure.Messaging;

/// <summary>
/// Servicio para manejar idempotencia de mensajes usando Redis
/// </summary>
public interface IIdempotencyService
{
    Task<bool> IsProcessedAsync(string idempotencyKey, CancellationToken cancellationToken = default);
    Task MarkAsProcessedAsync(string idempotencyKey, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
}

public class RedisIdempotencyService : IIdempotencyService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisIdempotencyService> _logger;
    private const string KeyPrefix = "idempotency:";
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromDays(7);

    public RedisIdempotencyService(
        IConnectionMultiplexer redis,
        ILogger<RedisIdempotencyService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<bool> IsProcessedAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = GetKey(idempotencyKey);
            return await db.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando idempotencia para clave {IdempotencyKey}", idempotencyKey);
            // En caso de error con Redis, permitir el procesamiento (fail-open)
            return false;
        }
    }

    public async Task MarkAsProcessedAsync(string idempotencyKey, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = GetKey(idempotencyKey);
            var exp = expiration ?? DefaultExpiration;
            
            await db.StringSetAsync(key, DateTime.UtcNow.ToString("O"), exp);
            
            _logger.LogDebug("Mensaje marcado como procesado: {IdempotencyKey}, expira en {Expiration}", 
                idempotencyKey, exp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marcando mensaje como procesado: {IdempotencyKey}", idempotencyKey);
            // No lanzar excepción para no interrumpir el flujo
        }
    }

    private static string GetKey(string idempotencyKey) => $"{KeyPrefix}{idempotencyKey}";
}
