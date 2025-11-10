using GateKeep.Api.Domain.Enums;

namespace GateKeep.Api.Contracts.Acceso;

public record CrearReglaAccesoRequest
{
    public required DateTime HorarioApertura { get; init; }
    public required DateTime HorarioCierre { get; init; }
    public required DateTime VigenciaApertura { get; init; }
    public required DateTime VigenciaCierre { get; init; }
    public required IReadOnlyList<Rol> RolesPermitidos { get; init; }
    public required long EspacioId { get; init; }
}

