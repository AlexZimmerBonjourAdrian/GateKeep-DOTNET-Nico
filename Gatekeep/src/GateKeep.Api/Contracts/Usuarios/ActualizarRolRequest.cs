using GateKeep.Api.Domain.Enums;

namespace GateKeep.Api.Contracts.Usuarios;

public record ActualizarRolRequest
{
    public Rol Rol { get; init; }
}

