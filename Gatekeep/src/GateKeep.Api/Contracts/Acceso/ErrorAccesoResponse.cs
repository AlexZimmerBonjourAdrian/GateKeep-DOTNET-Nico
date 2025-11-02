namespace GateKeep.Api.Contracts.Acceso;

public record ErrorAccesoResponse
{
    public required string TipoError { get; init; }
    public required string Mensaje { get; init; }
    public required string CodigoError { get; init; }
    public long? UsuarioId { get; init; }
    public long? EspacioId { get; init; }
    public string? PuntoControl { get; init; }
    public Dictionary<string, object>? DetallesAdicionales { get; init; }
    public DateTime Fecha { get; init; } = DateTime.UtcNow;
}

