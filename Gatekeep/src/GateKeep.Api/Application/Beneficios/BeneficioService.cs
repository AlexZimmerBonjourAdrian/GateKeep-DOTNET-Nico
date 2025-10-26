using GateKeep.Api.Contracts.Beneficios;
using GateKeep.Api.Domain.Entities;

namespace GateKeep.Api.Application.Beneficios;

public sealed class BeneficioService : IBeneficioService
{
    private readonly IBeneficioRepository _repository;

    public BeneficioService(IBeneficioRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<BeneficioDto>> ObtenerTodosAsync()
    {
        var beneficios = await _repository.ObtenerTodosAsync();
        return beneficios.Select(MapToDto);
    }

    public async Task<BeneficioDto?> ObtenerPorIdAsync(long id)
    {
        var beneficio = await _repository.ObtenerPorIdAsync(id);
        return beneficio is not null ? MapToDto(beneficio) : null;
    }

    public async Task<BeneficioDto> CrearAsync(CrearBeneficioRequest request)
    {
        var beneficio = new Beneficio(
            Id: 0, // Se asignará automáticamente
            Tipo: request.Tipo,
            Vigencia: request.Vigencia,
            FechaDeVencimiento: request.FechaDeVencimiento,
            Cupos: request.Cupos
        );

        var beneficioCreado = await _repository.CrearAsync(beneficio);
        return MapToDto(beneficioCreado);
    }

    public async Task<BeneficioDto> ActualizarAsync(long id, ActualizarBeneficioRequest request)
    {
        var beneficioExistente = await _repository.ObtenerPorIdAsync(id);
        if (beneficioExistente is null)
            throw new InvalidOperationException($"Beneficio con ID {id} no encontrado");

        var beneficioActualizado = beneficioExistente with
        {
            Tipo = request.Tipo,
            Vigencia = request.Vigencia,
            FechaDeVencimiento = request.FechaDeVencimiento,
            Cupos = request.Cupos
        };

        var resultado = await _repository.ActualizarAsync(beneficioActualizado);
        return MapToDto(resultado);
    }

    public async Task EliminarAsync(long id)
    {
        await _repository.EliminarAsync(id);
    }

    private static BeneficioDto MapToDto(Beneficio beneficio)
    {
        return new BeneficioDto
        {
            Id = beneficio.Id,
            Tipo = beneficio.Tipo,
            Vigencia = beneficio.Vigencia,
            FechaDeVencimiento = beneficio.FechaDeVencimiento,
            Cupos = beneficio.Cupos
        };
    }
}
