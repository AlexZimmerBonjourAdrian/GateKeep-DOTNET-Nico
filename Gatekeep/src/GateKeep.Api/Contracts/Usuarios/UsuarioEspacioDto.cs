namespace GateKeep.Api.Contracts.Usuarios;

public sealed record UsuarioEspacioDto
{
    public long UsuarioId { get; init; }
    public long EspacioId { get; init; }
}


