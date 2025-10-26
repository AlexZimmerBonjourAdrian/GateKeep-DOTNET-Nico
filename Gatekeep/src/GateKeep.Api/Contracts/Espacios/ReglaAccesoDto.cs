using GateKeep.Api.Domain.Enums;

namespace GateKeep.Api.Contracts.Espacios;

public record ReglaAccesoDto
{
    public long Id { get; init; }
    public DateTime HorarioApertura { get; init; }
    public DateTime HorarioCierre { get; init; }
    public DateTime VigenciaApertura { get; init; }
    public DateTime VigenciaCierre { get; init; }
    public IReadOnlyList<Rol> RolesPermitidos { get; init; } = Array.Empty<Rol>();
    public long EspacioId { get; init; }
}


