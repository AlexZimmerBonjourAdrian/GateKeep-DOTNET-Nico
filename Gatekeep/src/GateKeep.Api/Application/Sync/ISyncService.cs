using GateKeep.Api.Contracts.Sync;

namespace GateKeep.Api.Application.Sync;

/// <summary>
/// Interfaz para el servicio de sincronización offline-first
/// </summary>
public interface ISyncService
{
    /// <summary>
    /// Procesa una solicitud de sincronización desde un cliente
    /// </summary>
    /// <param name="request">Solicitud con eventos offline capturados</param>
    /// <param name="usuarioId">ID del usuario solicitante</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Respuesta de sincronización con datos actualizados</returns>
    Task<SyncResponse> SyncAsync(SyncRequest request, long usuarioId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene los datos que debe sincronizar un usuario específico
    /// </summary>
    /// <param name="usuarioId">ID del usuario</param>
    /// <param name="lastSyncTime">Timestamp de la última sincronización (para delta sync)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Payload de datos para sincronizar</returns>
    Task<SyncDataPayload> GetDataToSyncAsync(long usuarioId, DateTime? lastSyncTime = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra o actualiza el timestamp de sincronización de un dispositivo
    /// </summary>
    /// <param name="deviceId">ID del dispositivo</param>
    /// <param name="usuarioId">ID del usuario</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task RegisterDeviceSyncAsync(string deviceId, long usuarioId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Procesa un evento offline individual
    /// </summary>
    /// <param name="offlineEvent">Evento a procesar</param>
    /// <param name="usuarioId">ID del usuario propietario del evento</param>
    /// <param name="deviceId">ID del dispositivo origen</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado del procesamiento</returns>
    Task<SyncedEventResult> ProcessOfflineEventAsync(OfflineEventDto offlineEvent, long usuarioId, string deviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reintentar procesar eventos offline que fallaron
    /// </summary>
    /// <param name="maxRetries">Número máximo de reintentos</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task RetryFailedEventsAsync(int maxRetries = 3, CancellationToken cancellationToken = default);
}
