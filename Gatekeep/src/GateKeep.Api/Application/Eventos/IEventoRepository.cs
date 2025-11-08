using GateKeep.Api.Domain.Entities;

namespace GateKeep.Api.Application.Eventos;

public interface IEventoRepository
{
    Task<IEnumerable<Evento>> ObtenerTodosAsync();
    Task<Evento?> ObtenerPorIdAsync(long id);
    Task<Evento> CrearAsync(Evento evento);
    Task<Evento> ActualizarAsync(Evento evento);
    Task EliminarAsync(long id);
}

