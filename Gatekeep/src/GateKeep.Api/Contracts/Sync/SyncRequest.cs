namespace GateKeep.Api.Contracts.Sync;

/// <summary>
/// Contrato de solicitud de sincronización desde cliente
/// </summary>
public class SyncRequest
{
    /// <summary>
    /// Identificador único del dispositivo cliente
    /// </summary>
    public required string DeviceId { get; set; }

    /// <summary>
    /// Timestamp de la última sincronización exitosa del cliente
    /// </summary>
    public DateTime? LastSyncTime { get; set; }

    /// <summary>
    /// Eventos locales capturados sin conectividad
    /// </summary>
    public List<OfflineEventDto> PendingEvents { get; set; } = new();

    /// <summary>
    /// Versión del cliente para compatibilidad futura
    /// </summary>
    public string ClientVersion { get; set; } = "1.0.0";
}

/// <summary>
/// DTO de un evento capturado offline
/// </summary>
public class OfflineEventDto
{
    /// <summary>
    /// ID único del evento en la BD local del cliente
    /// </summary>
    public required string IdTemporal { get; set; }

    /// <summary>
    /// Tipo de evento (Acceso, Beneficio, Notificación, etc.)
    /// </summary>
    public required string EventType { get; set; }

    /// <summary>
    /// Datos del evento en formato JSON
    /// </summary>
    public required string EventData { get; set; }

    /// <summary>
    /// Timestamp local de cuando se creó el evento
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Intento actual de sincronización
    /// </summary>
    public int AttemptCount { get; set; } = 1;
}
