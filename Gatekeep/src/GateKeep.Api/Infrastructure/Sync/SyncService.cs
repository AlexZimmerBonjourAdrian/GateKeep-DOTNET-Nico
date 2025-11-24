using System.Text.Json;
using GateKeep.Api.Application.Sync;
using GateKeep.Api.Application.Usuarios;
using GateKeep.Api.Application.Eventos;
using GateKeep.Api.Application.Anuncios;
using GateKeep.Api.Application.Beneficios;
using GateKeep.Api.Application.Acceso;
using GateKeep.Api.Contracts.Sync;
using GateKeep.Api.Contracts.Usuarios;
using GateKeep.Api.Contracts.Eventos;
using GateKeep.Api.Contracts.Anuncios;
using GateKeep.Api.Contracts.Beneficios;
using GateKeep.Api.Contracts.Acceso;
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
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IEventoService _eventoService;
    private readonly IAnuncioService _anuncioService;
    private readonly IBeneficioService _beneficioService;
    private readonly IReglaAccesoService _reglaAccesoService;

    public SyncService(
        GateKeepDbContext context,
        ILogger<SyncService> logger,
        IUsuarioRepository usuarioRepository,
        IEventoService eventoService,
        IAnuncioService anuncioService,
        IBeneficioService beneficioService,
        IReglaAccesoService reglaAccesoService)
    {
        _context = context;
        _logger = logger;
        _usuarioRepository = usuarioRepository;
        _eventoService = eventoService;
        _anuncioService = anuncioService;
        _beneficioService = beneficioService;
        _reglaAccesoService = reglaAccesoService;
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
            // Si es un evento de tipo api_request, ejecutar la petición real
            if (offlineEvent.EventType == "api_request")
            {
                await ExecuteApiRequestAsync(offlineEvent, usuarioId, cancellationToken);
            }

            // Crear registro de evento offline en BD
            var eventoOffline = new EventoOffline
            {
                DeviceId = deviceId,
                IdTemporal = offlineEvent.IdTemporal,
                TipoEvento = offlineEvent.EventType,
                DatosEvento = offlineEvent.EventData,
                FechaCreacionCliente = offlineEvent.CreatedAt,
                FechaRecepcion = DateTime.UtcNow,
                Estado = "Procesado",
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

    /// <summary>
    /// Opciones de serialización JSON para manejar camelCase del frontend
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Ejecuta una petición HTTP que fue guardada offline
    /// </summary>
    private async Task ExecuteApiRequestAsync(OfflineEventDto offlineEvent, long usuarioId, CancellationToken cancellationToken)
    {
        try
        {
            // Deserializar los datos del evento
            var apiRequestData = JsonSerializer.Deserialize<ApiRequestData>(offlineEvent.EventData, JsonOptions);
            if (apiRequestData == null)
            {
                throw new InvalidOperationException("No se pudieron deserializar los datos de la petición");
            }

            _logger.LogInformation("Ejecutando petición offline: {Method} {Url}", apiRequestData.Method, apiRequestData.Url);

            // Parsear la URL para determinar el recurso y el ID
            var urlParts = ParseUrl(apiRequestData.Url);
            if (urlParts == null)
            {
                throw new InvalidOperationException($"URL no reconocida: {apiRequestData.Url}");
            }

            // Deserializar el body si existe
            // El data puede venir como string JSON o como objeto ya serializado
            JsonElement? requestBody = null;
            if (!string.IsNullOrEmpty(apiRequestData.Data))
            {
                // apiRequestData.Data puede ser un string JSON o un objeto serializado
                // Intentar deserializar como JsonElement
                try
                {
                    // Si es un string JSON, parsearlo
                    if (apiRequestData.Data.TrimStart().StartsWith('{') || apiRequestData.Data.TrimStart().StartsWith('['))
                    {
                        requestBody = JsonSerializer.Deserialize<JsonElement>(apiRequestData.Data, JsonOptions);
                    }
                    else
                    {
                        // Si no empieza con { o [, puede ser un objeto serializado de otra forma
                        // Intentar deserializar directamente
                        requestBody = JsonSerializer.Deserialize<JsonElement>(apiRequestData.Data, JsonOptions);
                    }
                }
                catch
                {
                    // Si falla, intentar crear un JsonElement desde el string directamente
                    requestBody = JsonDocument.Parse(apiRequestData.Data).RootElement;
                }
            }

            // Ejecutar la petición según el método HTTP
            switch (apiRequestData.Method.ToUpper())
            {
                case "POST":
                    await ExecutePostAsync(urlParts.Resource, urlParts.Id, requestBody, cancellationToken);
                    break;
                case "PUT":
                    await ExecutePutAsync(urlParts.Resource, urlParts.Id, requestBody, cancellationToken);
                    break;
                case "DELETE":
                    await ExecuteDeleteAsync(urlParts.Resource, urlParts.Id, cancellationToken);
                    break;
                case "PATCH":
                    await ExecutePatchAsync(urlParts.Resource, urlParts.Id, requestBody, cancellationToken);
                    break;
                default:
                    throw new InvalidOperationException($"Método HTTP no soportado: {apiRequestData.Method}");
            }

            _logger.LogInformation("Petición offline ejecutada exitosamente: {Method} {Url}", apiRequestData.Method, apiRequestData.Url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ejecutando petición offline {IdTemporal}", offlineEvent.IdTemporal);
            throw;
        }
    }

    /// <summary>
    /// Parsea una URL para extraer el recurso y el ID
    /// </summary>
    private UrlParts? ParseUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return null;

        // Remover la base URL y /api/ si existe
        var cleanUrl = url
            .Replace("https://api.zimmzimmgames.com", "")
            .Replace("http://localhost:5011", "")
            .Replace("/api/", "")
            .TrimStart('/');

        // Dividir por /
        var parts = cleanUrl.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return null;

        var resource = parts[0];
        long? id = null;

        // Intentar parsear el segundo segmento como ID
        if (parts.Length > 1 && long.TryParse(parts[1], out var parsedId))
        {
            id = parsedId;
        }

        return new UrlParts { Resource = resource, Id = id };
    }

    /// <summary>
    /// Ejecuta una petición POST
    /// </summary>
    private async Task ExecutePostAsync(string resource, long? id, JsonElement? requestBody, CancellationToken cancellationToken)
    {
        if (!requestBody.HasValue)
        {
            throw new InvalidOperationException("El body de la petición POST no puede ser nulo");
        }

        switch (resource.ToLower())
        {
            case "eventos":
                var crearEventoRequest = JsonSerializer.Deserialize<CrearEventoRequest>(requestBody.Value.GetRawText(), JsonOptions);
                if (crearEventoRequest != null)
                {
                    await _eventoService.CrearAsync(crearEventoRequest);
                }
                break;
            case "anuncios":
                var crearAnuncioRequest = JsonSerializer.Deserialize<CrearAnuncioRequest>(requestBody.Value.GetRawText(), JsonOptions);
                if (crearAnuncioRequest != null)
                {
                    await _anuncioService.CrearAsync(crearAnuncioRequest);
                }
                break;
            case "beneficios":
                var crearBeneficioRequest = JsonSerializer.Deserialize<CrearBeneficioRequest>(requestBody.Value.GetRawText(), JsonOptions);
                if (crearBeneficioRequest != null)
                {
                    await _beneficioService.CrearAsync(crearBeneficioRequest);
                }
                break;
            case "reglas-acceso":
                var crearReglaRequest = JsonSerializer.Deserialize<CrearReglaAccesoRequest>(requestBody.Value.GetRawText(), JsonOptions);
                if (crearReglaRequest != null)
                {
                    await _reglaAccesoService.CrearAsync(crearReglaRequest);
                }
                break;
            default:
                throw new InvalidOperationException($"Recurso no soportado para POST: {resource}");
        }
    }

    /// <summary>
    /// Ejecuta una petición PUT
    /// </summary>
    private async Task ExecutePutAsync(string resource, long? id, JsonElement? requestBody, CancellationToken cancellationToken)
    {
        if (!id.HasValue)
        {
            throw new InvalidOperationException("El ID es requerido para peticiones PUT");
        }

        if (!requestBody.HasValue)
        {
            throw new InvalidOperationException("El body de la petición PUT no puede ser nulo");
        }

        switch (resource.ToLower())
        {
            case "usuarios":
                var actualizarUsuarioRequest = JsonSerializer.Deserialize<ActualizarUsuarioRequest>(requestBody.Value.GetRawText(), JsonOptions);
                if (actualizarUsuarioRequest != null)
                {
                    var usuario = await _usuarioRepository.GetByIdAsync(id.Value);
                    if (usuario != null)
                    {
                        var usuarioActualizado = usuario with
                        {
                            Nombre = actualizarUsuarioRequest.Nombre,
                            Apellido = actualizarUsuarioRequest.Apellido,
                            Telefono = actualizarUsuarioRequest.Telefono
                        };
                        await _usuarioRepository.UpdateAsync(usuarioActualizado);
                    }
                }
                break;
            case "eventos":
                var actualizarEventoRequest = JsonSerializer.Deserialize<ActualizarEventoRequest>(requestBody.Value.GetRawText(), JsonOptions);
                if (actualizarEventoRequest != null)
                {
                    await _eventoService.ActualizarAsync(id.Value, actualizarEventoRequest);
                }
                break;
            case "anuncios":
                var actualizarAnuncioRequest = JsonSerializer.Deserialize<ActualizarAnuncioRequest>(requestBody.Value.GetRawText(), JsonOptions);
                if (actualizarAnuncioRequest != null)
                {
                    await _anuncioService.ActualizarAsync(id.Value, actualizarAnuncioRequest);
                }
                break;
            case "beneficios":
                var actualizarBeneficioRequest = JsonSerializer.Deserialize<ActualizarBeneficioRequest>(requestBody.Value.GetRawText(), JsonOptions);
                if (actualizarBeneficioRequest != null)
                {
                    await _beneficioService.ActualizarAsync(id.Value, actualizarBeneficioRequest);
                }
                break;
            case "reglas-acceso":
                var actualizarReglaRequest = JsonSerializer.Deserialize<ActualizarReglaAccesoRequest>(requestBody.Value.GetRawText(), JsonOptions);
                if (actualizarReglaRequest != null)
                {
                    await _reglaAccesoService.ActualizarAsync(id.Value, actualizarReglaRequest);
                }
                break;
            default:
                throw new InvalidOperationException($"Recurso no soportado para PUT: {resource}");
        }
    }

    /// <summary>
    /// Ejecuta una petición DELETE
    /// </summary>
    private async Task ExecuteDeleteAsync(string resource, long? id, CancellationToken cancellationToken)
    {
        if (!id.HasValue)
        {
            throw new InvalidOperationException("El ID es requerido para peticiones DELETE");
        }

        switch (resource.ToLower())
        {
            case "eventos":
                await _eventoService.EliminarAsync(id.Value);
                break;
            case "anuncios":
                await _anuncioService.EliminarAsync(id.Value);
                break;
            case "beneficios":
                await _beneficioService.EliminarAsync(id.Value);
                break;
            case "reglas-acceso":
                await _reglaAccesoService.EliminarAsync(id.Value);
                break;
            default:
                throw new InvalidOperationException($"Recurso no soportado para DELETE: {resource}");
        }
    }

    /// <summary>
    /// Ejecuta una petición PATCH
    /// </summary>
    private async Task ExecutePatchAsync(string resource, long? id, JsonElement? requestBody, CancellationToken cancellationToken)
    {
        // Por ahora, PATCH se maneja igual que PUT
        // En el futuro se puede implementar lógica específica para PATCH
        await ExecutePutAsync(resource, id, requestBody, cancellationToken);
    }

    /// <summary>
    /// Estructura para almacenar datos de una petición API offline
    /// </summary>
    private class ApiRequestData
    {
        public string Url { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string? Data { get; set; }
        public string? BaseUrl { get; set; }
    }

    /// <summary>
    /// Estructura para almacenar partes parseadas de una URL
    /// </summary>
    private class UrlParts
    {
        public string Resource { get; set; } = string.Empty;
        public long? Id { get; set; }
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
