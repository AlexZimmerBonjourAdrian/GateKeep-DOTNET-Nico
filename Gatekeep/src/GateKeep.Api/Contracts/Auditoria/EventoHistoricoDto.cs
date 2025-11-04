namespace GateKeep.Api.Contracts.Auditoria;

public record EventoHistoricoDto
{
    public string Id { get; init; } = string.Empty;
    public string TipoEvento { get; init; } = string.Empty;
    public DateTime Fecha { get; init; }
    public long UsuarioId { get; init; }
    public long? EspacioId { get; init; }
    public string Resultado { get; init; } = string.Empty;
    public string? PuntoControl { get; init; }
    public Dictionary<string, object>? Datos { get; init; }
}

