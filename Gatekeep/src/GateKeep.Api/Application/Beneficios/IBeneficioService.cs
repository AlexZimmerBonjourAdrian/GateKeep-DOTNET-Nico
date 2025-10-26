using GateKeep.Api.Contracts.Beneficios;

namespace GateKeep.Api.Application.Beneficios;

public interface IBeneficioService
{
    Task<IEnumerable<BeneficioDto>> ObtenerTodosAsync();
    Task<BeneficioDto?> ObtenerPorIdAsync(long id);
    Task<BeneficioDto> CrearAsync(CrearBeneficioRequest request);
    Task<BeneficioDto> ActualizarAsync(long id, ActualizarBeneficioRequest request);
    Task EliminarAsync(long id);
}
