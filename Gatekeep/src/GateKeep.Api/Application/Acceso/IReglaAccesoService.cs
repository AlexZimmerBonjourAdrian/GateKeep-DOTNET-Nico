using GateKeep.Api.Contracts.Acceso;

namespace GateKeep.Api.Application.Acceso;

public interface IReglaAccesoService
{
    Task<IEnumerable<ReglaAccesoDto>> ObtenerTodasAsync();
    Task<ReglaAccesoDto?> ObtenerPorIdAsync(long id);
    Task<ReglaAccesoDto?> ObtenerPorEspacioIdAsync(long espacioId);
    Task<ReglaAccesoDto> CrearAsync(CrearReglaAccesoRequest request);
    Task<ReglaAccesoDto> ActualizarAsync(long id, ActualizarReglaAccesoRequest request);
    Task EliminarAsync(long id);
}

