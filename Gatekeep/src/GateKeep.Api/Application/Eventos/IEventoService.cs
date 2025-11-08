using GateKeep.Api.Contracts.Eventos;

namespace GateKeep.Api.Application.Eventos;

public interface IEventoService
{
    Task<IEnumerable<EventoDto>> ObtenerTodosAsync();
    Task<EventoDto?> ObtenerPorIdAsync(long id);
    Task<EventoDto> CrearAsync(CrearEventoRequest request);
    Task<EventoDto> ActualizarAsync(long id, ActualizarEventoRequest request);
    Task EliminarAsync(long id);
}

