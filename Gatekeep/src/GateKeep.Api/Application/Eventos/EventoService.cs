using GateKeep.Api.Contracts.Eventos;
using GateKeep.Api.Domain.Entities;

namespace GateKeep.Api.Application.Eventos;

public sealed class EventoService : IEventoService
{
    private readonly IEventoRepository _repository;

    public EventoService(IEventoRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<EventoDto>> ObtenerTodosAsync()
    {
        var eventos = await _repository.ObtenerTodosAsync();
        return eventos.Select(MapToDto);
    }

    public async Task<EventoDto?> ObtenerPorIdAsync(long id)
    {
        var evento = await _repository.ObtenerPorIdAsync(id);
        return evento is not null ? MapToDto(evento) : null;
    }

    public async Task<EventoDto> CrearAsync(CrearEventoRequest request)
    {
        var evento = new Evento(
            Id: 0,
            Nombre: request.Nombre,
            Fecha: request.Fecha,
            Resultado: request.Resultado,
            PuntoControl: request.PuntoControl,
            Activo: true
        );

        var eventoCreado = await _repository.CrearAsync(evento);
        return MapToDto(eventoCreado);
    }

    public async Task<EventoDto> ActualizarAsync(long id, ActualizarEventoRequest request)
    {
        var eventoExistente = await _repository.ObtenerPorIdAsync(id);
        if (eventoExistente is null)
            throw new InvalidOperationException($"Evento con ID {id} no encontrado");

        var eventoActualizado = eventoExistente with
        {
            Nombre = request.Nombre,
            Fecha = request.Fecha,
            Resultado = request.Resultado,
            PuntoControl = request.PuntoControl,
            Activo = request.Activo
        };

        var resultado = await _repository.ActualizarAsync(eventoActualizado);
        return MapToDto(resultado);
    }

    public async Task EliminarAsync(long id)
    {
        await _repository.EliminarAsync(id);
    }

    private static EventoDto MapToDto(Evento evento)
    {
        return new EventoDto
        {
            Id = evento.Id,
            Nombre = evento.Nombre,
            Fecha = evento.Fecha,
            Resultado = evento.Resultado,
            PuntoControl = evento.PuntoControl,
            Activo = evento.Activo
        };
    }
}

