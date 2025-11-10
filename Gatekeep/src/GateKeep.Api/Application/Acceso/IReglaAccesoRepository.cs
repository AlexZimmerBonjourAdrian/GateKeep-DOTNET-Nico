using GateKeep.Api.Domain.Entities;

namespace GateKeep.Api.Application.Acceso;

public interface IReglaAccesoRepository
{
    Task<ReglaAcceso?> ObtenerPorIdAsync(long id);
    Task<ReglaAcceso?> ObtenerPorEspacioIdAsync(long espacioId);
    Task<ReglaAcceso> CrearAsync(ReglaAcceso reglaAcceso);
    Task<ReglaAcceso> ActualizarAsync(ReglaAcceso reglaAcceso);
    Task<bool> EliminarAsync(long id);
    Task<IEnumerable<ReglaAcceso>> ObtenerTodasAsync();
}

