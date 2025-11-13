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

    /// <summary>
    /// Registra el inicio de una sincronización
    /// </summary>
    void RecordSincronizacionIniciada(string tipo);

    /// <summary>
    /// Registra la finalización de una sincronización
    /// </summary>
    void RecordSincronizacionCompletada(string tipo);

    /// <summary>
    /// Registra un evento que falló al procesarse
    /// </summary>
    void RecordEventoError(string tipoEvento);

    /// <summary>
    /// Actualiza la métrica de sincronizaciones pendientes
    /// </summary>
    void UpdateSincronizacionesPendientes(string tipo, int cantidad);

    /// <summary>
    /// Actualiza la métrica de eventos pendientes
    /// </summary>
    void UpdateEventosPendientes(string tipoEvento, int cantidad);
}

