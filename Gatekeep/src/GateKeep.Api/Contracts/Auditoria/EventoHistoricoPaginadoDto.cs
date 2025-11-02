namespace GateKeep.Api.Contracts.Auditoria;

public record EventoHistoricoPaginadoDto
{
    public required IEnumerable<EventoHistoricoDto> Eventos { get; init; }
    public required PaginacionDto Paginacion { get; init; }
}

public record PaginacionDto
{
    public int Pagina { get; init; }
    public int TamanoPagina { get; init; }
    public long TotalCount { get; init; }
    public int TotalPaginas { get; init; }
}

