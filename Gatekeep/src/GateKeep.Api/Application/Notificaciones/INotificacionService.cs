using GateKeep.Api.Contracts.Notificaciones;

namespace GateKeep.Api.Application.Notificaciones;

public interface INotificacionService
{
    Task<NotificacionDto> CrearNotificacionAsync(string mensaje, string tipo = "general", long? usuarioIdCreador = null);
    Task<IEnumerable<NotificacionDto>> ObtenerTodasAsync();
    Task<NotificacionDto?> ObtenerPorIdAsync(string id);
    Task<NotificacionDto?> ActualizarNotificacionAsync(string id, string mensaje, string tipo);
    Task<bool> EliminarNotificacionAsync(string id);
}
