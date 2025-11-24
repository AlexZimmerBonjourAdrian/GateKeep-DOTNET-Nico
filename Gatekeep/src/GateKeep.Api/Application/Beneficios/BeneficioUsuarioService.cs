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
        var dtos = new List<BeneficioUsuarioDto>();
        
        foreach (var bu in beneficiosUsuarios)
        {
            var dto = await MapToDtoAsync(bu);
            dtos.Add(dto);
        }
        
        return dtos;
    }

    public async Task<IEnumerable<BeneficioUsuarioDto>> ObtenerBeneficiosCanjeadosPorUsuarioAsync(long usuarioId)
    {
        var beneficiosUsuarios = await _repository.ObtenerPorUsuarioAsync(usuarioId);
        var canjeados = beneficiosUsuarios.Where(b => b.EstadoCanje);
        var dtos = new List<BeneficioUsuarioDto>();
        
        foreach (var bu in canjeados)
        {
            var dto = await MapToDtoAsync(bu);
            dtos.Add(dto);
        }
        
        return dtos.OrderByDescending(d => d.FechaCanje);
    }

    public async Task<IEnumerable<BeneficioUsuarioDto>> ObtenerUsuariosPorBeneficioAsync(long beneficioId)
    {
        var beneficiosUsuarios = await _repository.ObtenerPorBeneficioAsync(beneficioId);
        var dtos = new List<BeneficioUsuarioDto>();
        
        foreach (var bu in beneficiosUsuarios)
        {
            var dto = await MapToDtoAsync(bu);
            dtos.Add(dto);
        }
        
        return dtos;
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

        return await MapToDtoAsync(resultado);
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

    public async Task<BeneficioUsuarioDto> CanjearBeneficioAsync(long usuarioId, long beneficioId, string puntoControl)
    {
        // Obtener información del beneficio primero
        var beneficio = await _beneficioRepository.ObtenerPorIdAsync(beneficioId);
        if (beneficio is null)
        {
            throw new InvalidOperationException("Beneficio no encontrado");
        }

        // Verificar vigencia
        if (!beneficio.Vigencia || beneficio.FechaDeVencimiento < DateTime.UtcNow)
        {
            throw new InvalidOperationException("El beneficio no está vigente o ha vencido");
        }

        // Verificar cupos disponibles
        if (beneficio.Cupos <= 0)
        {
            throw new InvalidOperationException("No hay cupos disponibles para este beneficio");
        }

        // Verificar si el usuario ya tiene este beneficio
        var beneficioUsuario = await _repository.ObtenerAsync(usuarioId, beneficioId);
        
        if (beneficioUsuario is null)
        {
            // Si no lo tiene, asignarlo automáticamente
            beneficioUsuario = new BeneficioUsuario(
                UsuarioId: usuarioId,
                BeneficioId: beneficioId,
                EstadoCanje: false
            );
            beneficioUsuario = await _repository.CrearAsync(beneficioUsuario);
        }
        else if (beneficioUsuario.EstadoCanje)
        {
            // Si ya lo canjeó antes, no permitir canje duplicado
            throw new InvalidOperationException("Ya has canjeado este beneficio anteriormente");
        }

        var fecha = DateTime.UtcNow;
        
        // Actualizar estado de canje con fecha
        var beneficioActualizado = beneficioUsuario with { EstadoCanje = true, FechaCanje = fecha };
        await _repository.ActualizarAsync(beneficioActualizado);

        // Decrementar cupos disponibles del beneficio
        var beneficioConCuposActualizados = beneficio with { Cupos = beneficio.Cupos - 1 };
        await _beneficioRepository.ActualizarAsync(beneficioConCuposActualizados);

        var beneficioNombre = $"Beneficio {beneficio.Tipo}";

        // Notificar a observers (Observer Pattern)
        if (_eventPublisher != null)
        {
            try
            {
                await _eventPublisher.NotifyBeneficioCanjeadoAsync(
                    usuarioId, 
                    beneficioId, 
                    beneficioNombre, 
                    puntoControl, 
                    fecha);
            }
            catch
            {
                // Log error pero no romper el flujo principal
            }
        }

        return await MapToDtoAsync(beneficioActualizado);
    }

    private async Task<BeneficioUsuarioDto> MapToDtoAsync(BeneficioUsuario beneficioUsuario)
    {
        var beneficio = await _beneficioRepository.ObtenerPorIdAsync(beneficioUsuario.BeneficioId);
        
        return new BeneficioUsuarioDto
        {
            UsuarioId = beneficioUsuario.UsuarioId,
            BeneficioId = beneficioUsuario.BeneficioId,
            EstadoCanje = beneficioUsuario.EstadoCanje,
            FechaCanje = beneficioUsuario.FechaCanje,
            Tipo = beneficio?.Tipo,
            Vigencia = beneficio?.Vigencia,
            FechaDeVencimiento = beneficio?.FechaDeVencimiento
        };
    }
}
