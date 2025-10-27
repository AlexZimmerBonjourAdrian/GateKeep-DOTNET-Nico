using GateKeep.Api.Domain.Enums;

namespace GateKeep.Api.Domain.Entities;
public sealed record class Estudiante(
  long Id,
  string Email,
  string Nombre,
  string Apellido,
  string Contrasenia,
  string? Telefono,
  DateTime FechaAlta,
  TipoCredencial Credencial
) : Usuario(Id, Email, Nombre, Apellido, Contrasenia, Telefono, FechaAlta, Rol.Estudiante, Credencial);

