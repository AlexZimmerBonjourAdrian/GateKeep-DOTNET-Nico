namespace GateKeep.Api.Infrastructure.Observability;

/// <summary>
/// Servicio para registrar métricas de negocio y observabilidad
/// </summary>
public interface IObservabilityService
{
    /// <summary>
    /// Registra un acceso al sistema
    /// </summary>
    void RecordAcceso(string tipoAcceso, bool exitoso);

    /// <summary>
    /// Registra la creación de un evento
    /// </summary>
    void RecordEventoCreado(string tipoEvento);

    /// <summary>
    /// Registra el envío de una notificación
    /// </summary>
    void RecordNotificacionEnviada(string canal, bool exitosa);

    /// <summary>
    /// Registra una operación de base de datos
    /// </summary>
    void RecordDatabaseOperation(string database, string operation, double durationMs);

    /// <summary>
    /// Registra una operación de cache
    /// </summary>
    void RecordCacheOperation(string operation, bool hit);

    /// <summary>
    /// Registra un error en el sistema
    /// </summary>
    void RecordError(string component, string errorType);
}

