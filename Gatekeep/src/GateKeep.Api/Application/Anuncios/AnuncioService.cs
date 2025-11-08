using GateKeep.Api.Contracts.Anuncios;
using GateKeep.Api.Domain.Entities;

namespace GateKeep.Api.Application.Anuncios;

public sealed class AnuncioService : IAnuncioService
{
    private readonly IAnuncioRepository _repository;

    public AnuncioService(IAnuncioRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<AnuncioDto>> ObtenerTodosAsync()
    {
        var anuncios = await _repository.ObtenerTodosAsync();
        return anuncios.Select(MapToDto);
    }

    public async Task<AnuncioDto?> ObtenerPorIdAsync(long id)
    {
        var anuncio = await _repository.ObtenerPorIdAsync(id);
        return anuncio is not null ? MapToDto(anuncio) : null;
    }

    public async Task<AnuncioDto> CrearAsync(CrearAnuncioRequest request)
    {
        var anuncio = new Anuncio(
            Id: 0,
            Nombre: request.Nombre,
            Fecha: request.Fecha,
            Activo: true
        );

        var anuncioCreado = await _repository.CrearAsync(anuncio);
        return MapToDto(anuncioCreado);
    }

    public async Task<AnuncioDto> ActualizarAsync(long id, ActualizarAnuncioRequest request)
    {
        var anuncioExistente = await _repository.ObtenerPorIdAsync(id);
        if (anuncioExistente is null)
            throw new InvalidOperationException($"Anuncio con ID {id} no encontrado");

        var anuncioActualizado = anuncioExistente with
        {
            Nombre = request.Nombre,
            Fecha = request.Fecha,
            Activo = request.Activo
        };

        var resultado = await _repository.ActualizarAsync(anuncioActualizado);
        return MapToDto(resultado);
    }

    public async Task EliminarAsync(long id)
    {
        await _repository.EliminarAsync(id);
    }

    private static AnuncioDto MapToDto(Anuncio anuncio)
    {
        return new AnuncioDto
        {
            Id = anuncio.Id,
            Nombre = anuncio.Nombre,
            Fecha = anuncio.Fecha,
            Activo = anuncio.Activo
        };
    }
}

