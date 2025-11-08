using GateKeep.Api.Domain.Entities;

namespace GateKeep.Api.Application.Anuncios;

public interface IAnuncioRepository
{
    Task<IEnumerable<Anuncio>> ObtenerTodosAsync();
    Task<Anuncio?> ObtenerPorIdAsync(long id);
    Task<Anuncio> CrearAsync(Anuncio anuncio);
    Task<Anuncio> ActualizarAsync(Anuncio anuncio);
    Task EliminarAsync(long id);
}

