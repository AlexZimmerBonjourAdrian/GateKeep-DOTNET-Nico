namespace GateKeep.Api.Contracts.Acceso;

public record ValidarAccesoResponse
{
    public bool Permitido { get; init; }
    public string? Razon { get; init; }
    public long UsuarioId { get; init; }
    public long EspacioId { get; init; }
    public string PuntoControl { get; init; } = string.Empty;
    public DateTime Fecha { get; init; } = DateTime.UtcNow;
}

