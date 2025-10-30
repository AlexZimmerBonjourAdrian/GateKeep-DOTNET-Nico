using GateKeep.Api.Domain.Enums;

namespace GateKeep.Api.Contracts.Usuarios;

public record UsuarioDto
{
    public long Id { get; init; } = 0;
    public required string Email { get; init; }
    public required string Nombre { get; init; }
    public required string Apellido { get; init; }
    public required string Contrasenia { get; init; }
    public string? Telefono { get; init; }
    public DateTime FechaAlta { get; init; } = DateTime.UtcNow;
    public TipoCredencial Credencial { get; init; } = TipoCredencial.Vigente;
    public TipoUsuario TipoUsuario { get; init; } = TipoUsuario.Estudiante;
}


