using GateKeep.Api.Domain.Entities;

namespace GateKeep.Api.Application.Notificaciones;

public interface INotificacionRepository
{
    Task<IEnumerable<Notificacion>> ObtenerTodasAsync();
    Task<Notificacion?> ObtenerPorIdAsync(string id);
    Task<Notificacion> CrearAsync(Notificacion notificacion);
    Task<Notificacion> ActualizarAsync(Notificacion notificacion);
    Task<bool> EliminarAsync(string id);
    Task<IEnumerable<Notificacion>> ObtenerActivasAsync();
    Task<IEnumerable<Notificacion>> ObtenerPorTipoAsync(string tipo);
}
