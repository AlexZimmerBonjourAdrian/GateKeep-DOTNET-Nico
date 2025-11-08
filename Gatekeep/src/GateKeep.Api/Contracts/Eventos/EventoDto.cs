namespace GateKeep.Api.Contracts.Eventos;

public record EventoDto
{
    public long Id { get; init; }
    public required string Nombre { get; init; }
    public DateTime Fecha { get; init; }
    public required string Resultado { get; init; }
    public required string PuntoControl { get; init; }
    public bool Activo { get; init; }
}

