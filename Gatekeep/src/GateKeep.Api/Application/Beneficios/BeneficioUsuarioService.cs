using GateKeep.Api.Contracts.Beneficios;
using GateKeep.Api.Domain.Entities;

namespace GateKeep.Api.Application.Beneficios;

public sealed class BeneficioUsuarioService : IBeneficioUsuarioService
{
    private readonly IBeneficioUsuarioRepository _repository;

    public BeneficioUsuarioService(IBeneficioUsuarioRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<BeneficioUsuarioDto>> ObtenerBeneficiosPorUsuarioAsync(long usuarioId)
    {
        var beneficiosUsuarios = await _repository.ObtenerPorUsuarioAsync(usuarioId);
        return beneficiosUsuarios.Select(MapToDto);
    }

    public async Task<IEnumerable<BeneficioUsuarioDto>> ObtenerUsuariosPorBeneficioAsync(long beneficioId)
    {
        var beneficiosUsuarios = await _repository.ObtenerPorBeneficioAsync(beneficioId);
        return beneficiosUsuarios.Select(MapToDto);
    }

    public async Task<BeneficioUsuarioDto> AsignarBeneficioAsync(long usuarioId, long beneficioId)
    {
        var beneficioUsuario = new BeneficioUsuario(
            UsuarioId: usuarioId,
            BeneficioId: beneficioId,
            EstadoCanje: false
        );

        var resultado = await _repository.CrearAsync(beneficioUsuario);
        return MapToDto(resultado);
    }

    public async Task DesasignarBeneficioAsync(long usuarioId, long beneficioId)
    {
        await _repository.EliminarAsync(usuarioId, beneficioId);
    }

    public async Task<bool> TieneBeneficioAsync(long usuarioId, long beneficioId)
    {
        var beneficioUsuario = await _repository.ObtenerAsync(usuarioId, beneficioId);
        return beneficioUsuario is not null;
    }

    private static BeneficioUsuarioDto MapToDto(BeneficioUsuario beneficioUsuario)
    {
        return new BeneficioUsuarioDto
        {
            UsuarioId = beneficioUsuario.UsuarioId,
            BeneficioId = beneficioUsuario.BeneficioId,
            EstadoCanje = beneficioUsuario.EstadoCanje
        };
    }
}
