namespace GateKeep.Api.Domain.Events;

/// <summary>
/// Evento de dominio que se publica cuando se canjea un beneficio
/// </summary>
public record BeneficioCanjeadoEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public long UsuarioId { get; init; }
    public long BeneficioId { get; init; }
    public string NombreBeneficio { get; init; } = string.Empty;
    public string PuntoControl { get; init; } = string.Empty;
    public int PuntosCanjeados { get; init; }
    
    // Para idempotencia
    public string IdempotencyKey { get; init; } = string.Empty;
    
    public BeneficioCanjeadoEvent() { }
    
    public BeneficioCanjeadoEvent(
        long usuarioId, 
        long beneficioId,
        string nombreBeneficio,
        string puntoControl,
        int puntosCanjeados)
    {
        UsuarioId = usuarioId;
        BeneficioId = beneficioId;
        NombreBeneficio = nombreBeneficio;
        PuntoControl = puntoControl;
        PuntosCanjeados = puntosCanjeados;
        IdempotencyKey = GenerateIdempotencyKey(usuarioId, beneficioId, Timestamp);
    }
    
    private static string GenerateIdempotencyKey(long usuarioId, long beneficioId, DateTime timestamp)
    {
        var roundedTimestamp = new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, 
            timestamp.Hour, timestamp.Minute, timestamp.Second, DateTimeKind.Utc);
        return $"beneficio-canjeado-{usuarioId}-{beneficioId}-{roundedTimestamp:yyyyMMddHHmmss}";
    }
}
