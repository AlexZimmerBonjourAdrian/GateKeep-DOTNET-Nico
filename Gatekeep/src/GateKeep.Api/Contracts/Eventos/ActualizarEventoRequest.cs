namespace GateKeep.Api.Contracts.Eventos;

public record ActualizarEventoRequest
{
    public required string Nombre { get; init; }
    public DateTime Fecha { get; init; }
    public required string Resultado { get; init; }
    public required string PuntoControl { get; init; }
    public bool Activo { get; init; } = true;
}

