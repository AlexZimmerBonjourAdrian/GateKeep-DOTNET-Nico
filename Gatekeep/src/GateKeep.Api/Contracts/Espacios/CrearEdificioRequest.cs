namespace GateKeep.Api.Contracts.Espacios;

/// <summary>
/// DTO para crear edificios
/// </summary>
public record CrearEdificioRequest : CrearEspacioRequest
{
    public int NumeroPisos { get; init; }
    public string? CodigoEdificio { get; init; }
}
