namespace GateKeep.Api.Contracts.Espacios;

/// <summary>
/// DTO para crear salones
/// </summary>
public record CrearSalonRequest : CrearEspacioRequest
{
    public long EdificioId { get; init; }
    public int NumeroSalon { get; init; }
    public string? TipoSalon { get; init; }
}
