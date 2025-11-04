using GateKeep.Api.Contracts.Notificaciones;

namespace GateKeep.Api.Application.Notificaciones;

public interface INotificacionUsuarioService
{
    Task<IEnumerable<NotificacionCompletaDto>> ObtenerNotificacionesPorUsuarioAsync(long usuarioId);
    Task<NotificacionCompletaDto?> ObtenerNotificacionCompletaAsync(long usuarioId, string notificacionId);
    Task<bool> MarcarComoLeidaAsync(long usuarioId, string notificacionId);
    Task<int> ContarNoLeidasAsync(long usuarioId);
}

