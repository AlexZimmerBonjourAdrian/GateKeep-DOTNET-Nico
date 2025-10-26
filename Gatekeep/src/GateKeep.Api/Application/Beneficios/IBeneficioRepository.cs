using GateKeep.Api.Domain.Entities;

namespace GateKeep.Api.Application.Beneficios;

public interface IBeneficioRepository
{
    Task<IEnumerable<Beneficio>> ObtenerTodosAsync();
    Task<Beneficio?> ObtenerPorIdAsync(long id);
    Task<Beneficio> CrearAsync(Beneficio beneficio);
    Task<Beneficio> ActualizarAsync(Beneficio beneficio);
    Task EliminarAsync(long id);
}
