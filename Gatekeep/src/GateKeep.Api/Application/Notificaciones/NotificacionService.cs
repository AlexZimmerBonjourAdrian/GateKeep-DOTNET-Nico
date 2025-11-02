using GateKeep.Api.Application.Auditoria;
using GateKeep.Api.Application.Notificaciones;
using GateKeep.Api.Contracts.Notificaciones;
using GateKeep.Api.Domain.Entities;

namespace GateKeep.Api.Application.Notificaciones;

public class NotificacionService : INotificacionService
{
    private readonly INotificacionRepository _notificacionRepository;
    private readonly IEventoHistoricoService? _eventoHistoricoService;

    public NotificacionService(
        INotificacionRepository notificacionRepository,
        IEventoHistoricoService? eventoHistoricoService = null)
    {
        _notificacionRepository = notificacionRepository;
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
            catch
            {
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
