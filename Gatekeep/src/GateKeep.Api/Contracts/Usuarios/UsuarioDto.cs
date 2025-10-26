using GateKeep.Api.Domain.Enums;

namespace GateKeep.Api.Contracts.Usuarios;

public record UsuarioDto
{
    public long Id { get; init; }
    public required string Email { get; init; }
    public required string Nombre { get; init; }
    public required string Apellido { get; init; }
    public required string Contrasenia { get; init; }
    public string? Telefono { get; init; }
    public DateTime FechaAlta { get; init; }
    public Rol Rol { get; init; }
    public TipoCredencial Credencial { get; init; }
}


