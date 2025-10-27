using GateKeep.Api.Domain.Entities;

namespace GateKeep.Api.Application.Notificaciones;

public interface INotificacionUsuarioRepository
{
    Task<IEnumerable<NotificacionUsuario>> ObtenerPorUsuarioAsync(long usuarioId);
    Task<NotificacionUsuario?> ObtenerPorUsuarioYNotificacionAsync(long usuarioId, string notificacionId);
    Task<NotificacionUsuario> CrearAsync(NotificacionUsuario notificacionUsuario);
    Task<NotificacionUsuario> ActualizarAsync(NotificacionUsuario notificacionUsuario);
    Task<bool> MarcarComoLeidaAsync(long usuarioId, string notificacionId);
    Task<IEnumerable<NotificacionUsuario>> ObtenerNoLeidasAsync(long usuarioId);
    Task<int> ContarNoLeidasAsync(long usuarioId);
}
