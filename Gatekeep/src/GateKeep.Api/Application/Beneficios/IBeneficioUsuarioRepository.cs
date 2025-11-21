using GateKeep.Api.Domain.Entities;

namespace GateKeep.Api.Application.Beneficios;

public interface IBeneficioUsuarioRepository
{
    Task<IEnumerable<(BeneficioUsuario BeneficioUsuario, Beneficio? Beneficio)>> ObtenerPorUsuarioConDetallesAsync(long usuarioId);
    Task<IEnumerable<BeneficioUsuario>> ObtenerPorUsuarioAsync(long usuarioId);
    Task<IEnumerable<BeneficioUsuario>> ObtenerPorBeneficioAsync(long beneficioId);
    Task<BeneficioUsuario?> ObtenerAsync(long usuarioId, long beneficioId);
    Task<BeneficioUsuario> CrearAsync(BeneficioUsuario beneficioUsuario);
    Task<BeneficioUsuario> ActualizarAsync(BeneficioUsuario beneficioUsuario);
    Task EliminarAsync(long usuarioId, long beneficioId);
}
