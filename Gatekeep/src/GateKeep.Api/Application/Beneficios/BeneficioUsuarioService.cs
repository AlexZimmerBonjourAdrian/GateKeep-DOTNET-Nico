using GateKeep.Api.Application.Events;
using GateKeep.Api.Contracts.Beneficios;
using GateKeep.Api.Domain.Entities;

namespace GateKeep.Api.Application.Beneficios;

public sealed class BeneficioUsuarioService : IBeneficioUsuarioService
{
    private readonly IBeneficioUsuarioRepository _repository;
    private readonly IBeneficioRepository _beneficioRepository;
    private readonly IEventPublisher? _eventPublisher;

    public BeneficioUsuarioService(
        IBeneficioUsuarioRepository repository,
        IBeneficioRepository beneficioRepository,
        IEventPublisher? eventPublisher = null)
    {
        _repository = repository;
        _beneficioRepository = beneficioRepository;
        _eventPublisher = eventPublisher;
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
        var fecha = DateTime.UtcNow;

        // Notificar a observers (Observer Pattern)
        if (_eventPublisher != null)
        {
            try
            {
                var beneficio = await _beneficioRepository.ObtenerPorIdAsync(beneficioId);
                var beneficioNombre = beneficio != null ? $"Beneficio {beneficio.Tipo}" : $"Beneficio {beneficioId}";
                
                await _eventPublisher.NotifyBeneficioAsignadoAsync(usuarioId, beneficioId, beneficioNombre, fecha);
            }
            catch
            {
                // Log error pero no romper el flujo principal
            }
        }

        return MapToDto(resultado);
    }

    public async Task DesasignarBeneficioAsync(long usuarioId, long beneficioId)
    {
        await _repository.EliminarAsync(usuarioId, beneficioId);
        var fecha = DateTime.UtcNow;

        // Notificar a observers (Observer Pattern)
        if (_eventPublisher != null)
        {
            try
            {
                var beneficio = await _beneficioRepository.ObtenerPorIdAsync(beneficioId);
                var beneficioNombre = beneficio != null ? $"Beneficio {beneficio.Tipo}" : $"Beneficio {beneficioId}";
                
                await _eventPublisher.NotifyBeneficioDesasignadoAsync(usuarioId, beneficioId, beneficioNombre, fecha);
            }
            catch
            {
                // Log error pero no romper el flujo principal
            }
        }
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
