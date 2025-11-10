using GateKeep.Api.Contracts.Acceso;
using GateKeep.Api.Domain.Entities;

namespace GateKeep.Api.Application.Acceso;

public sealed class ReglaAccesoService : IReglaAccesoService
{
    private readonly IReglaAccesoRepository _repository;

    public ReglaAccesoService(IReglaAccesoRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ReglaAccesoDto>> ObtenerTodasAsync()
    {
        var reglas = await _repository.ObtenerTodasAsync();
        return reglas.Select(MapToDto);
    }

    public async Task<ReglaAccesoDto?> ObtenerPorIdAsync(long id)
    {
        var regla = await _repository.ObtenerPorIdAsync(id);
        return regla is not null ? MapToDto(regla) : null;
    }

    public async Task<ReglaAccesoDto?> ObtenerPorEspacioIdAsync(long espacioId)
    {
        var regla = await _repository.ObtenerPorEspacioIdAsync(espacioId);
        return regla is not null ? MapToDto(regla) : null;
    }

    public async Task<ReglaAccesoDto> CrearAsync(CrearReglaAccesoRequest request)
    {
        var regla = new ReglaAcceso(
            Id: 0,
            HorarioApertura: request.HorarioApertura,
            HorarioCierre: request.HorarioCierre,
            VigenciaApertura: request.VigenciaApertura,
            VigenciaCierre: request.VigenciaCierre,
            RolesPermitidos: request.RolesPermitidos,
            EspacioId: request.EspacioId
        );

        var reglaCreada = await _repository.CrearAsync(regla);
        return MapToDto(reglaCreada);
    }

    public async Task<ReglaAccesoDto> ActualizarAsync(long id, ActualizarReglaAccesoRequest request)
    {
        var reglaExistente = await _repository.ObtenerPorIdAsync(id);
        if (reglaExistente is null)
            throw new InvalidOperationException($"ReglaAcceso con ID {id} no encontrado");

        var reglaActualizada = reglaExistente with
        {
            HorarioApertura = request.HorarioApertura,
            HorarioCierre = request.HorarioCierre,
            VigenciaApertura = request.VigenciaApertura,
            VigenciaCierre = request.VigenciaCierre,
            RolesPermitidos = request.RolesPermitidos,
            EspacioId = request.EspacioId
        };

        var resultado = await _repository.ActualizarAsync(reglaActualizada);
        return MapToDto(resultado);
    }

    public async Task EliminarAsync(long id)
    {
        var eliminado = await _repository.EliminarAsync(id);
        if (!eliminado)
            throw new InvalidOperationException($"ReglaAcceso con ID {id} no encontrado");
    }

    private static ReglaAccesoDto MapToDto(ReglaAcceso regla)
    {
        return new ReglaAccesoDto
        {
            Id = regla.Id,
            HorarioApertura = regla.HorarioApertura,
            HorarioCierre = regla.HorarioCierre,
            VigenciaApertura = regla.VigenciaApertura,
            VigenciaCierre = regla.VigenciaCierre,
            RolesPermitidos = regla.RolesPermitidos,
            EspacioId = regla.EspacioId
        };
    }
}

