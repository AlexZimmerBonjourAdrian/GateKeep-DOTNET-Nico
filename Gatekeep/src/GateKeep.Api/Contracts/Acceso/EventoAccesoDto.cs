namespace GateKeep.Api.Contracts.Acceso;

public record EventoAccesoDto
{
    public long Id { get; init; }
    public required string Nombre { get; init; }
    public DateTime Fecha { get; init; }
    public required string Resultado { get; init; }
    public required string PuntoControl { get; init; }
    public long UsuarioId { get; init; }
    public long EspacioId { get; init; }
}


