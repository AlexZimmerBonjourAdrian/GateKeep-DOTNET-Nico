using GateKeep.Api.Contracts.Eventos;
using GateKeep.Api.Domain.Entities;
using GateKeep.Api.Infrastructure.Observability;
using Microsoft.Extensions.Logging;

namespace GateKeep.Api.Application.Eventos;

public sealed class EventoService : IEventoService
{
    private readonly IEventoRepository _repository;
    private readonly IObservabilityService _observabilityService;
    private readonly ILogger<EventoService> _logger;

    public EventoService(
        IEventoRepository repository,
        IObservabilityService observabilityService,
        ILogger<EventoService> logger)
    {
        _repository = repository;
        _observabilityService = observabilityService;
        _logger = logger;
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
        
        // Registrar métrica de evento creado
        _observabilityService.RecordEventoCreado(request.Resultado ?? "General");
        _logger.LogInformation("Evento creado: Nombre={Nombre}, Resultado={Resultado}, PuntoControl={PuntoControl}",
            request.Nombre, request.Resultado, request.PuntoControl);
        
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

