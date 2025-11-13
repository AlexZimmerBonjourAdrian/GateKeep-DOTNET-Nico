namespace GateKeep.Api.Domain.Events;

/// <summary>
/// Evento de dominio que se publica cuando se rechaza un acceso
/// </summary>
public record AccesoRechazadoEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public long UsuarioId { get; init; }
    public long? EspacioId { get; init; }
    public string Razon { get; init; } = string.Empty;
    public string PuntoControl { get; init; } = string.Empty;
    public string TipoError { get; init; } = string.Empty;
    public Dictionary<string, object>? DetallesAdicionales { get; init; }
    
    // Para idempotencia
    public string IdempotencyKey { get; init; } = string.Empty;
    
    public AccesoRechazadoEvent() { }
    
    public AccesoRechazadoEvent(
        long usuarioId, 
        long? espacioId, 
        string razon, 
        string puntoControl,
        string tipoError,
        Dictionary<string, object>? detallesAdicionales = null)
    {
        UsuarioId = usuarioId;
        EspacioId = espacioId;
        Razon = razon;
        PuntoControl = puntoControl;
        TipoError = tipoError;
        DetallesAdicionales = detallesAdicionales;
        // Generar clave de idempotencia única basada en los datos del evento
        IdempotencyKey = GenerateIdempotencyKey(usuarioId, espacioId, puntoControl, Timestamp);
    }
    
    private static string GenerateIdempotencyKey(long usuarioId, long? espacioId, string puntoControl, DateTime timestamp)
    {
        // Redondear timestamp a segundo para evitar duplicados en la misma ventana de tiempo
        var roundedTimestamp = new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, 
            timestamp.Hour, timestamp.Minute, timestamp.Second, DateTimeKind.Utc);
        return $"acceso-rechazado-{usuarioId}-{espacioId}-{puntoControl}-{roundedTimestamp:yyyyMMddHHmmss}";
    }
}
