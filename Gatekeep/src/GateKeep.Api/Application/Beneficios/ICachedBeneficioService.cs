using GateKeep.Api.Contracts.Beneficios;

namespace GateKeep.Api.Application.Beneficios;

/// <summary>
/// Servicio de beneficios con soporte de caching
/// </summary>
public interface ICachedBeneficioService
{
    Task<IEnumerable<BeneficioDto>> ObtenerTodosAsync();
    Task<BeneficioDto?> ObtenerPorIdAsync(long id);
    Task<IEnumerable<BeneficioDto>> ObtenerBeneficiosVigentesAsync();
    Task<BeneficioDto> CrearAsync(CrearBeneficioRequest request);
    Task<BeneficioDto> ActualizarAsync(long id, ActualizarBeneficioRequest request);
    Task EliminarAsync(long id);
}

