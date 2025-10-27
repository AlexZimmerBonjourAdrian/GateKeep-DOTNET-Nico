using GateKeep.Api.Contracts.Notificaciones;

namespace GateKeep.Api.Application.Notificaciones;

public interface INotificacionService
{
    Task<NotificacionDto> CrearNotificacionAsync(string mensaje, string tipo = "general");
    Task<IEnumerable<NotificacionDto>> ObtenerTodasAsync();
    Task<NotificacionDto?> ObtenerPorIdAsync(string id);
    Task<bool> EliminarNotificacionAsync(string id);
}
