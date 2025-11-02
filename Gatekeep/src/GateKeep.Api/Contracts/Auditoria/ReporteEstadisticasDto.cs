namespace GateKeep.Api.Contracts.Auditoria;

public record ReporteEstadisticasDto
{
    public DateTime FechaDesde { get; init; }
    public DateTime FechaHasta { get; init; }
    public required Dictionary<string, long> EstadisticasPorTipo { get; init; }
}

