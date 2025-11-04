using GateKeep.Api.Domain.Enums;

namespace GateKeep.Api.Domain.Entities;

public sealed record ReglaAcceso
(
    long Id,
    DateTime HorarioApertura,
    DateTime HorarioCierre,
    DateTime VigenciaApertura,
    DateTime VigenciaCierre,
    IReadOnlyList<Rol> RolesPermitidos,
    long EspacioId
);


