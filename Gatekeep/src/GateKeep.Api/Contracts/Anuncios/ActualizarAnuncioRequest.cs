namespace GateKeep.Api.Contracts.Anuncios;

public record ActualizarAnuncioRequest
{
    public required string Nombre { get; init; }
    public DateTime Fecha { get; init; }
    public string? Descripcion { get; init; }
    public string? PuntoControl { get; init; }
    public bool Activo { get; init; } = true;
}

