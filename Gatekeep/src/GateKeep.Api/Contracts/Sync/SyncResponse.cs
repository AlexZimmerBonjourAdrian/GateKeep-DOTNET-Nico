namespace GateKeep.Api.Contracts.Sync;

/// <summary>
/// Contrato de respuesta de sincronización hacia el cliente
/// </summary>
public class SyncResponse
{
    /// <summary>
    /// Timestamp del servidor (para sincronizar relojes)
    /// </summary>
    public DateTime ServerTime { get; set; }

    /// <summary>
    /// True si la sincronización fue exitosa
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Mensaje de estado o error
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Resultado de procesamiento de cada evento offline
    /// </summary>
    public List<SyncedEventResult> ProcessedEvents { get; set; } = new();

    /// <summary>
    /// Datos actualizados que el cliente debe sincronizar
    /// (usuarios, espacios, reglas, beneficios, notificaciones, etc.)
    /// </summary>
    public SyncDataPayload? DataToSync { get; set; }

    /// <summary>
    /// Token JWT renovado si corresponde
    /// </summary>
    public string? NewAuthToken { get; set; }

    /// <summary>
    /// Última sincronización exitosa registrada en servidor
    /// </summary>
    public DateTime LastSuccessfulSync { get; set; }
}

/// <summary>
/// Resultado del procesamiento de un evento offline
/// </summary>
public class SyncedEventResult
{
    /// <summary>
    /// ID temporal del evento del cliente
    /// </summary>
    public required string IdTemporal { get; set; }

    /// <summary>
    /// True si fue procesado exitosamente
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// ID permanente asignado en el servidor (si aplica)
    /// </summary>
    public string? PermanentId { get; set; }

    /// <summary>
    /// Mensaje de error si falló el procesamiento
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Timestamp de cuando fue procesado en servidor
    /// </summary>
    public DateTime ProcessedAt { get; set; }
}

/// <summary>
/// Payload de datos a sincronizar en el cliente
/// </summary>
public class SyncDataPayload
{
    /// <summary>
    /// Usuarios (perfil, roles, estado de credencial)
    /// </summary>
    public List<UsuarioSyncDto> Usuarios { get; set; } = new();

    /// <summary>
    /// Espacios accesibles (salones, laboratorios, etc.)
    /// </summary>
    public List<EspacioSyncDto> Espacios { get; set; } = new();

    /// <summary>
    /// Reglas de acceso aplicables al usuario
    /// </summary>
    public List<ReglaAccesoSyncDto> ReglasAcceso { get; set; } = new();

    /// <summary>
    /// Beneficios disponibles y su estado
    /// </summary>
    public List<BeneficioSyncDto> Beneficios { get; set; } = new();

    /// <summary>
    /// Notificaciones nuevas o pendientes
    /// </summary>
    public List<NotificacionSyncDto> Notificaciones { get; set; } = new();
}

public class UsuarioSyncDto
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public bool CredentialActiva { get; set; }
    public DateTime UltimaActualizacion { get; set; }
}

public class EspacioSyncDto
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string? Ubicacion { get; set; }
    public DateTime UltimaActualizacion { get; set; }
}

public class ReglaAccesoSyncDto
{
    public long Id { get; set; }
    public long EspacioId { get; set; }
    public string Perfil { get; set; } = string.Empty;
    public string? HoraInicio { get; set; }
    public string? HoraFin { get; set; }
    public bool Activa { get; set; }
    public DateTime UltimaActualizacion { get; set; }
}

public class BeneficioSyncDto
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public DateTime? FechaVigenciaInicio { get; set; }
    public DateTime? FechaVigenciaFin { get; set; }
    public int CuposDisponibles { get; set; }
    public bool Activo { get; set; }
    public DateTime UltimaActualizacion { get; set; }
}

public class NotificacionSyncDto
{
    public string Id { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public bool Leido { get; set; }
    public DateTime FechaCreacion { get; set; }
}
