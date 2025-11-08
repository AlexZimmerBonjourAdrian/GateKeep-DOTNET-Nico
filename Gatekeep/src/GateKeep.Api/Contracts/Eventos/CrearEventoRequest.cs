namespace GateKeep.Api.Contracts.Eventos;

public record CrearEventoRequest
{
    public required string Nombre { get; init; }
    public DateTime Fecha { get; init; }
    public required string Resultado { get; init; }
    public required string PuntoControl { get; init; }
}

