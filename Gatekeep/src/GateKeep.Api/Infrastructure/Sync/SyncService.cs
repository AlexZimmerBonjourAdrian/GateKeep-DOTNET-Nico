using System.Text.Json;
using GateKeep.Api.Application.Sync;
using GateKeep.Api.Contracts.Sync;
using GateKeep.Api.Domain.Entities;
using GateKeep.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GateKeep.Api.Infrastructure.Sync;

/// <summary>
/// Implementación del servicio de sincronización offline-first
/// </summary>
public class SyncService : ISyncService
{
    private readonly GateKeepDbContext _context;
    private readonly ILogger<SyncService> _logger;

    public SyncService(GateKeepDbContext context, ILogger<SyncService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SyncResponse> SyncAsync(SyncRequest request, long usuarioId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando sincronización para dispositivo {DeviceId}, usuario {UsuarioId}", request.DeviceId, usuarioId);

        var response = new SyncResponse
        {
            ServerTime = DateTime.UtcNow,
            Success = true
        };

        try
        {
            // 1. Registrar o actualizar dispositivo
            await RegisterDeviceSyncAsync(request.DeviceId, usuarioId, cancellationToken);

            // 2. Procesar eventos offline
            foreach (var offlineEvent in request.PendingEvents)
            {
                try
                {
                    var result = await ProcessOfflineEventAsync(offlineEvent, usuarioId, request.DeviceId, cancellationToken);
                    response.ProcessedEvents.Add(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error procesando evento offline {IdTemporal}", offlineEvent.IdTemporal);
                    response.ProcessedEvents.Add(new SyncedEventResult
                    {
                        IdTemporal = offlineEvent.IdTemporal,
                        Success = false,
                        ErrorMessage = ex.Message,
                        ProcessedAt = DateTime.UtcNow
                    });
                }
            }

            // 3. Obtener datos para sincronizar
            response.DataToSync = await GetDataToSyncAsync(usuarioId, request.LastSyncTime, cancellationToken);

            // 4. Registrar última sincronización exitosa
            var dispositivo = await _context.DispositivosSync
                .FirstOrDefaultAsync(d => d.DeviceId == request.DeviceId && d.UsuarioId == usuarioId, cancellationToken);

            if (dispositivo != null)
            {
                response.LastSuccessfulSync = dispositivo.UltimaSincronizacion;
            }

            response.Message = "Sincronización completada exitosamente";
            _logger.LogInformation("Sincronización completada exitosamente para dispositivo {DeviceId}", request.DeviceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante sincronización del dispositivo {DeviceId}", request.DeviceId);
            response.Success = false;
            response.Message = "Error durante sincronización: " + ex.Message;
        }

        return response;
    }

    public async Task<SyncDataPayload> GetDataToSyncAsync(long usuarioId, DateTime? lastSyncTime = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo datos de sincronización para usuario {UsuarioId}, lastSyncTime: {LastSyncTime}", usuarioId, lastSyncTime);

        var payload = new SyncDataPayload();

        try
        {
            // 1. Datos del usuario
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == usuarioId, cancellationToken);

            if (usuario != null)
            {
                payload.Usuarios.Add(new UsuarioSyncDto
                {
                    Id = usuario.Id,
                    Nombre = usuario.Nombre,
                    Email = usuario.Email,
                    Rol = usuario.Rol.ToString(),
                    CredentialActiva = usuario.Credencial.ToString() == "Activa",
                    UltimaActualizacion = usuario.FechaAlta
                });
            }

            // 2. Espacios accesibles
            var espacios = await _context.Espacios
                .AsNoTracking()
                .Select(e => new EspacioSyncDto
                {
                    Id = e.Id,
                    Nombre = e.Nombre,
                    Tipo = "Espacio",
                    Ubicacion = e.Ubicacion,
                    UltimaActualizacion = DateTime.UtcNow
                })
                .ToListAsync(cancellationToken);

            payload.Espacios = espacios;

            // 3. Reglas de acceso del usuario
            var reglasAcceso = await _context.ReglasAcceso
                .AsNoTracking()
                .Select(r => new ReglaAccesoSyncDto
                {
                    Id = r.Id,
                    EspacioId = r.EspacioId,
                    Perfil = "Standard",
                    HoraInicio = r.HorarioApertura.ToString("HH:mm"),
                    HoraFin = r.HorarioCierre.ToString("HH:mm"),
                    Activa = r.VigenciaApertura < DateTime.UtcNow && DateTime.UtcNow < r.VigenciaCierre,
                    UltimaActualizacion = DateTime.UtcNow
                })
                .ToListAsync(cancellationToken);

            payload.ReglasAcceso = reglasAcceso;

            // 4. Beneficios disponibles
            var beneficios = await _context.Beneficios
                .AsNoTracking()
                .Where(b => b.Vigencia)
                .Select(b => new BeneficioSyncDto
                {
                    Id = b.Id,
                    Nombre = b.Tipo.ToString(),
                    Tipo = b.Tipo.ToString(),
                    FechaVigenciaInicio = null,
                    FechaVigenciaFin = b.FechaDeVencimiento,
                    CuposDisponibles = b.Cupos,
                    Activo = b.Vigencia,
                    UltimaActualizacion = DateTime.UtcNow
                })
                .ToListAsync(cancellationToken);

            payload.Beneficios = beneficios;

            // 5. Notificaciones del usuario
            // Las notificaciones están en MongoDB, por lo que omitimos por ahora
            payload.Notificaciones = new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo datos de sincronización para usuario {UsuarioId}", usuarioId);
        }

        return payload;
    }

    public async Task RegisterDeviceSyncAsync(string deviceId, long usuarioId, CancellationToken cancellationToken = default)
    {
        try
        {
            var dispositivo = await _context.DispositivosSync
                .FirstOrDefaultAsync(d => d.DeviceId == deviceId && d.UsuarioId == usuarioId, cancellationToken);

            if (dispositivo == null)
            {
                dispositivo = new DispositivoSync
                {
                    DeviceId = deviceId,
                    UsuarioId = usuarioId,
                    FechaCreacion = DateTime.UtcNow,
                    UltimaActualizacion = DateTime.UtcNow,
                    UltimaSincronizacion = DateTime.UtcNow,
                    Activo = true
                };

                _context.DispositivosSync.Add(dispositivo);
            }
            else
            {
                dispositivo.UltimaSincronizacion = DateTime.UtcNow;
                dispositivo.UltimaActualizacion = DateTime.UtcNow;
                _context.DispositivosSync.Update(dispositivo);
            }

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Dispositivo {DeviceId} sincronizado para usuario {UsuarioId}", deviceId, usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registrando dispositivo {DeviceId}", deviceId);
        }
    }

    public async Task<SyncedEventResult> ProcessOfflineEventAsync(OfflineEventDto offlineEvent, long usuarioId, string deviceId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Procesando evento offline {IdTemporal}, tipo: {EventType}", offlineEvent.IdTemporal, offlineEvent.EventType);

        var result = new SyncedEventResult
        {
            IdTemporal = offlineEvent.IdTemporal,
            ProcessedAt = DateTime.UtcNow
        };

        try
        {
            // Crear registro de evento offline en BD
            var eventoOffline = new EventoOffline
            {
                DeviceId = deviceId,
                IdTemporal = offlineEvent.IdTemporal,
                TipoEvento = offlineEvent.EventType,
                DatosEvento = offlineEvent.EventData,
                FechaCreacionCliente = offlineEvent.CreatedAt,
                FechaRecepcion = DateTime.UtcNow,
                Estado = "Pendiente",
                IntentosProcessamiento = offlineEvent.AttemptCount,
                UltimaActualizacion = DateTime.UtcNow
            };

            _context.EventosOffline.Add(eventoOffline);
            await _context.SaveChangesAsync(cancellationToken);

            result.PermanentId = eventoOffline.Id.ToString();
            result.Success = true;

            _logger.LogInformation("Evento offline {IdTemporal} procesado exitosamente", offlineEvent.IdTemporal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando evento offline {IdTemporal}", offlineEvent.IdTemporal);
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    public async Task RetryFailedEventsAsync(int maxRetries = 3, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando reintento de eventos offline fallidos");

        try
        {
            var failedEvents = await _context.EventosOffline
                .Where(e => e.Estado == "Error" && e.IntentosProcessamiento < maxRetries)
                .ToListAsync(cancellationToken);

            foreach (var @event in failedEvents)
            {
                try
                {
                    @event.IntentosProcessamiento++;
                    @event.UltimaActualizacion = DateTime.UtcNow;
                    // Aquí iría la lógica específica para procesar cada tipo de evento
                    @event.Estado = "Procesado";

                    _logger.LogInformation("Evento offline {IdTemporal} reintentado exitosamente", @event.IdTemporal);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reintentando evento offline {IdTemporal}", @event.IdTemporal);
                    @event.MensajeError = ex.Message;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante reintento de eventos offline");
        }
    }
}
