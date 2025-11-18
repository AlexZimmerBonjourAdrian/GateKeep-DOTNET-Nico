using GateKeep.Api.Application.Sync;
using GateKeep.Api.Contracts.Sync;
using GateKeep.Api.Endpoints.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GateKeep.Api.Endpoints.Sync;

/// <summary>
/// Endpoints para sincronización offline
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SyncController : ControllerBase
{
    private readonly ISyncService _syncService;
    private readonly ILogger<SyncController> _logger;

    public SyncController(ISyncService syncService, ILogger<SyncController> logger)
    {
        _syncService = syncService;
        _logger = logger;
    }

    /// <summary>
    /// Procesa una solicitud de sincronización desde un cliente
    /// </summary>
    /// <param name="request">Datos de sincronización con eventos offline</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Respuesta con datos sincronizados</returns>
    [HttpPost("")]
    public async Task<ActionResult<SyncResponse>> Sync(
        [FromBody] SyncRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sync request recibido para dispositivo {DeviceId}", request.DeviceId);

        // Extraer ID del usuario del token (implementar según tu autenticación)
        // Por ahora usamos un valor placeholder
        long usuarioId = 1; // TODO: Obtener del JWT token

        var response = await _syncService.SyncAsync(request, usuarioId, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Obtiene los datos de sincronización para el usuario actual
    /// </summary>
    /// <param name="lastSyncTime">Timestamp de última sincronización (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Payload con datos a sincronizar</returns>
    [HttpGet("data")]
    public async Task<ActionResult<SyncDataPayload>> GetDataToSync(
        [FromQuery] DateTime? lastSyncTime = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetDataToSync request, lastSyncTime: {LastSyncTime}", lastSyncTime);

        // TODO: Obtener del JWT token
        long usuarioId = 1;

        var payload = await _syncService.GetDataToSyncAsync(usuarioId, lastSyncTime, cancellationToken);
        return Ok(payload);
    }

    /// <summary>
    /// Registra un dispositivo para sincronización
    /// </summary>
    /// <param name="deviceId">Identificador del dispositivo</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Estado del registro</returns>
    [HttpPost("device/{deviceId}")]
    public async Task<ActionResult> RegisterDevice(
        [FromRoute] string deviceId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Registrando dispositivo {DeviceId}", deviceId);

        // TODO: Obtener del JWT token
        long usuarioId = 1;

        await _syncService.RegisterDeviceSyncAsync(deviceId, usuarioId, cancellationToken);
        return Ok(new { message = "Dispositivo registrado exitosamente" });
    }
}
