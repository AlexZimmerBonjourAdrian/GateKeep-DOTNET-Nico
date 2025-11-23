namespace GateKeep.Api.Contracts.Espacios;

/// <summary>
/// DTO base para crear espacios
/// </summary>
public record CrearEspacioRequest
{
    public required string Nombre { get; init; }
    public string? Descripcion { get; init; }
    public required string Ubicacion { get; init; }
    public int Capacidad { get; init; }
}
