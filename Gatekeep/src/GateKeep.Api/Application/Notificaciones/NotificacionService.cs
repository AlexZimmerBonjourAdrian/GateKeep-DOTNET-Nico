using GateKeep.Api.Application.Auditoria;
using GateKeep.Api.Application.Notificaciones;
using GateKeep.Api.Contracts.Notificaciones;
using GateKeep.Api.Domain.Entities;
using GateKeep.Api.Infrastructure.Observability;
using Microsoft.Extensions.Logging;

namespace GateKeep.Api.Application.Notificaciones;

public class NotificacionService : INotificacionService
{
    private readonly INotificacionRepository _notificacionRepository;
    private readonly IEventoHistoricoService? _eventoHistoricoService;
    private readonly IObservabilityService _observabilityService;
    private readonly ILogger<NotificacionService> _logger;

    public NotificacionService(
        INotificacionRepository notificacionRepository,
        IObservabilityService observabilityService,
        ILogger<NotificacionService> logger,
        IEventoHistoricoService? eventoHistoricoService = null)
    {
        _notificacionRepository = notificacionRepository;
        _observabilityService = observabilityService;
        _logger = logger;
        _eventoHistoricoService = eventoHistoricoService;
    }

    public async Task<NotificacionDto> CrearNotificacionAsync(string mensaje, string tipo = "general", long? usuarioIdCreador = null)
    {
        var notificacion = new Notificacion
        {
            Mensaje = mensaje,
            Tipo = tipo,
            FechaEnvio = DateTime.UtcNow,
            Activa = true
        };

        var notificacionCreada = await _notificacionRepository.CrearAsync(notificacion);
        
        // Registrar métrica de notificación enviada
        _observabilityService.RecordNotificacionEnviada(tipo, true);
        _logger.LogInformation("Notificación creada: Tipo={Tipo}, Mensaje={Mensaje}",
            tipo, mensaje);
        
        if (_eventoHistoricoService != null && usuarioIdCreador.HasValue)
        {
            try
            {
                await _eventoHistoricoService.RegistrarNotificacionAsync(
                    usuarioIdCreador.Value,
                    tipo,
                    "Creada",
                    new Dictionary<string, object> { { "notificacionId", notificacionCreada.Id } });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al registrar notificación en histórico");
                _observabilityService.RecordError("NotificacionService", "HistoricoError");
            }
        }
        
        return MapToDto(notificacionCreada);
    }

    public async Task<IEnumerable<NotificacionDto>> ObtenerTodasAsync()
    {
        var notificaciones = await _notificacionRepository.ObtenerTodasAsync();
        return notificaciones.Select(MapToDto);
    }

    public async Task<NotificacionDto?> ObtenerPorIdAsync(string id)
    {
        var notificacion = await _notificacionRepository.ObtenerPorIdAsync(id);
        return notificacion != null ? MapToDto(notificacion) : null;
    }

    public async Task<bool> EliminarNotificacionAsync(string id)
    {
        return await _notificacionRepository.EliminarAsync(id);
    }

    private static NotificacionDto MapToDto(Notificacion notificacion)
    {
        return new NotificacionDto
        {
            Id = notificacion.Id,
            Mensaje = notificacion.Mensaje,
            FechaEnvio = notificacion.FechaEnvio,
            Tipo = notificacion.Tipo,
            Activa = notificacion.Activa
        };
    }
}
