namespace GateKeep.Api.Contracts.Anuncios;

public record CrearAnuncioRequest
{
    public required string Nombre { get; init; }
    public DateTime Fecha { get; init; }
}

