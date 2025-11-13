namespace GateKeep.Api.Contracts.Usuarios;

public record ActualizarUsuarioRequest
{
    public required string Nombre { get; init; }
    public required string Apellido { get; init; }
    public string? Telefono { get; init; }
}
