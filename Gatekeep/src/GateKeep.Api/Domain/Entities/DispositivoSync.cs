namespace GateKeep.Api.Domain.Entities;

/// <summary>
/// Entidad que registra el historial de sincronizaciones de dispositivos
/// </summary>
public class DispositivoSync
{
    /// <summary>
    /// Identificador único de la sincronización
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Identificador del dispositivo del cliente (MD5/SHA de hardware)
    /// </summary>
    public required string DeviceId { get; set; }

    /// <summary>
    /// ID del usuario propietario del dispositivo
    /// </summary>
    public long UsuarioId { get; set; }

    /// <summary>
    /// Timestamp de la última sincronización exitosa
    /// </summary>
    public DateTime UltimaSincronizacion { get; set; }

    /// <summary>
    /// Timestamp de creación del registro
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Timestamp de última actualización
    /// </summary>
    public DateTime UltimaActualizacion { get; set; }

    /// <summary>
    /// Indica si el dispositivo está activo
    /// </summary>
    public bool Activo { get; set; } = true;

    /// <summary>
    /// Plataforma del dispositivo (iOS, Android, Web, etc.)
    /// </summary>
    public string? Plataforma { get; set; }

    /// <summary>
    /// Versión del cliente
    /// </summary>
    public string? VersionCliente { get; set; }
}

/// <summary>
/// Entidad que registra los eventos capturados offline esperando sincronización
/// </summary>
public class EventoOffline
{
    /// <summary>
    /// Identificador único del evento
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// ID del dispositivo que generó el evento
    /// </summary>
    public required string DeviceId { get; set; }

    /// <summary>
    /// ID temporal asignado por el cliente para deduplicación
    /// </summary>
    public required string IdTemporal { get; set; }

    /// <summary>
    /// Tipo de evento (Acceso, Beneficio, Notificación, etc.)
    /// </summary>
    public required string TipoEvento { get; set; }

    /// <summary>
    /// Datos del evento serializado en JSON
    /// </summary>
    public required string DatosEvento { get; set; }

    /// <summary>
    /// Timestamp de creación del evento en el cliente
    /// </summary>
    public DateTime FechaCreacionCliente { get; set; }

    /// <summary>
    /// Timestamp de recepción en el servidor
    /// </summary>
    public DateTime FechaRecepcion { get; set; }

    /// <summary>
    /// Estado del evento: Pendiente, Procesado, Error
    /// </summary>
    public string Estado { get; set; } = "Pendiente"; // Pendiente, Procesado, Error

    /// <summary>
    /// Número de intentos de procesamiento
    /// </summary>
    public int IntentosProcessamiento { get; set; } = 0;

    /// <summary>
    /// Mensaje de error si el procesamiento falló
    /// </summary>
    public string? MensajeError { get; set; }

    /// <summary>
    /// Timestamp de última actualización
    /// </summary>
    public DateTime UltimaActualizacion { get; set; }

    /// <summary>
    /// ID permanente del evento procesado (si aplica)
    /// </summary>
    public long? IdEventoPermanente { get; set; }
}
