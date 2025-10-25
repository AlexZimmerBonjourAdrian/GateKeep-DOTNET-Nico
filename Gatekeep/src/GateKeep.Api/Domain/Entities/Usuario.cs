using GateKeep.Api.Domain.Enums;

namespace GateKeep.Api.Domain.Entities;

public sealed record Usuario
(
    long Id,
    string Email,
    string Nombre,
    string Apellido,
    string Contrasenia,
    string? Telefono,
    DateTime FechaAlta,
    Rol Rol,
    TipoCredencial Credencial
);


