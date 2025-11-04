using GateKeep.Api.Domain.Entities;
using MongoDB.Driver;

namespace GateKeep.Api.Application.Auditoria;

public interface IEventoHistoricoRepository
{
    Task<EventoHistorico> CrearAsync(EventoHistorico evento);
    Task CrearLoteAsync(IEnumerable<EventoHistorico> eventos);
    Task<(IEnumerable<EventoHistorico> Eventos, long TotalCount)> ObtenerFiltradosAsync(
        FilterDefinition<EventoHistorico> filtro,
        int pagina = 1,
        int tamanoPagina = 50,
        SortDefinition<EventoHistorico>? orden = null);
    Task<long> ContarAsync(FilterDefinition<EventoHistorico> filtro);
    Task<Dictionary<string, long>> ObtenerEstadisticasPorTipoAsync(
        DateTime fechaDesde,
        DateTime fechaHasta);
    Task<(IEnumerable<EventoHistorico> Eventos, long TotalCount)> ObtenerPorUsuarioAsync(
        long usuarioId,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        string? tipoEvento = null,
        int pagina = 1,
        int tamanoPagina = 50);
    Task InicializarIndicesAsync();
}

