namespace GateKeep.Api.Contracts.Espacios;

/// <summary>
/// DTO para crear laboratorios
/// </summary>
public record CrearLaboratorioRequest : CrearEspacioRequest
{
    public long EdificioId { get; init; }
    public int NumeroLaboratorio { get; init; }
    public string? TipoLaboratorio { get; init; }
    public bool EquipamientoEspecial { get; init; }
}
