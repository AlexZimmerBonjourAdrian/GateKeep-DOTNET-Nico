using GateKeep.Api.Contracts.Beneficios;

namespace GateKeep.Api.Application.Beneficios;

public interface IBeneficioUsuarioService
{
    Task<IEnumerable<BeneficioUsuarioDto>> ObtenerBeneficiosPorUsuarioAsync(long usuarioId);
    Task<IEnumerable<BeneficioUsuarioDto>> ObtenerUsuariosPorBeneficioAsync(long beneficioId);
    Task<BeneficioUsuarioDto> AsignarBeneficioAsync(long usuarioId, long beneficioId);
    Task DesasignarBeneficioAsync(long usuarioId, long beneficioId);
    Task<bool> TieneBeneficioAsync(long usuarioId, long beneficioId);
}
