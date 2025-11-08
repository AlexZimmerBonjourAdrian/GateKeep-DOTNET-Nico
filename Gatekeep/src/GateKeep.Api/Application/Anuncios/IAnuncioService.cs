using GateKeep.Api.Contracts.Anuncios;

namespace GateKeep.Api.Application.Anuncios;

public interface IAnuncioService
{
    Task<IEnumerable<AnuncioDto>> ObtenerTodosAsync();
    Task<AnuncioDto?> ObtenerPorIdAsync(long id);
    Task<AnuncioDto> CrearAsync(CrearAnuncioRequest request);
    Task<AnuncioDto> ActualizarAsync(long id, ActualizarAnuncioRequest request);
    Task EliminarAsync(long id);
}

