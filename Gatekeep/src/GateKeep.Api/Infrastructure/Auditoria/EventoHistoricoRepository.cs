using GateKeep.Api.Application.Auditoria;
using GateKeep.Api.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GateKeep.Api.Infrastructure.Auditoria;

public class EventoHistoricoRepository : IEventoHistoricoRepository
{
    private readonly IMongoCollection<EventoHistorico> _collection;
    private readonly ILogger<EventoHistoricoRepository> _logger;
    private bool _indicesInicializados = false;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public EventoHistoricoRepository(
        IMongoDatabase database,
        ILogger<EventoHistoricoRepository> logger)
    {
        _collection = database.GetCollection<EventoHistorico>("eventos_historicos");
        _logger = logger;
    }

    public async Task<EventoHistorico> CrearAsync(EventoHistorico evento)
    {
        await EnsureIndicesAsync();
        await _collection.InsertOneAsync(evento);
        return evento;
    }

    public async Task CrearLoteAsync(IEnumerable<EventoHistorico> eventos)
    {
        await EnsureIndicesAsync();
        await _collection.InsertManyAsync(eventos);
    }

    public async Task<(IEnumerable<EventoHistorico> Eventos, long TotalCount)> ObtenerFiltradosAsync(
        FilterDefinition<EventoHistorico> filtro,
        int pagina = 1,
        int tamanoPagina = 50,
        SortDefinition<EventoHistorico>? orden = null)
    {
        orden ??= Builders<EventoHistorico>.Sort.Descending(e => e.Fecha);

        var totalCount = await _collection.CountDocumentsAsync(filtro);

        var eventos = await _collection
            .Find(filtro)
            .Sort(orden)
            .Skip((pagina - 1) * tamanoPagina)
            .Limit(tamanoPagina)
            .ToListAsync();

        return (eventos, totalCount);
    }

    public async Task<long> ContarAsync(FilterDefinition<EventoHistorico> filtro)
    {
        return await _collection.CountDocumentsAsync(filtro);
    }

    public async Task<Dictionary<string, long>> ObtenerEstadisticasPorTipoAsync(
        DateTime fechaDesde,
        DateTime fechaHasta)
    {
        var pipeline = new BsonDocument[]
        {
            new BsonDocument("$match", new BsonDocument
            {
                { "fecha", new BsonDocument
                    {
                        { "$gte", fechaDesde },
                        { "$lte", fechaHasta }
                    }
                }
            }),
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", "$tipoEvento" },
                { "count", new BsonDocument("$sum", 1) }
            })
        };

        var resultados = await _collection.Aggregate<BsonDocument>(pipeline).ToListAsync();

        return resultados.ToDictionary(
            r => r["_id"].AsString,
            r => r["count"].AsInt64);
    }

    public async Task<(IEnumerable<EventoHistorico> Eventos, long TotalCount)> ObtenerPorUsuarioAsync(
        long usuarioId,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        string? tipoEvento = null,
        int pagina = 1,
        int tamanoPagina = 50)
    {
        var filtro = Builders<EventoHistorico>.Filter.Eq(e => e.UsuarioId, usuarioId);

        if (fechaDesde.HasValue)
            filtro &= Builders<EventoHistorico>.Filter.Gte(e => e.Fecha, fechaDesde.Value);

        if (fechaHasta.HasValue)
            filtro &= Builders<EventoHistorico>.Filter.Lte(e => e.Fecha, fechaHasta.Value);

        if (!string.IsNullOrEmpty(tipoEvento))
            filtro &= Builders<EventoHistorico>.Filter.Eq(e => e.TipoEvento, tipoEvento);

        return await ObtenerFiltradosAsync(filtro, pagina, tamanoPagina);
    }

    public async Task InicializarIndicesAsync()
    {
        await EnsureIndicesAsync();
    }

    private async Task EnsureIndicesAsync()
    {
        if (_indicesInicializados)
            return;

        await _semaphore.WaitAsync();
        try
        {
            if (_indicesInicializados)
                return;

            var indexModels = new List<CreateIndexModel<EventoHistorico>>
            {
                new CreateIndexModel<EventoHistorico>(
                    Builders<EventoHistorico>.IndexKeys.Ascending(e => e.Fecha),
                    new CreateIndexOptions { Name = "IX_fecha" }),

                new CreateIndexModel<EventoHistorico>(
                    Builders<EventoHistorico>.IndexKeys.Ascending(e => e.UsuarioId),
                    new CreateIndexOptions { Name = "IX_usuarioId" }),

                new CreateIndexModel<EventoHistorico>(
                    Builders<EventoHistorico>.IndexKeys.Ascending(e => e.TipoEvento),
                    new CreateIndexOptions { Name = "IX_tipoEvento" }),

                new CreateIndexModel<EventoHistorico>(
                    Builders<EventoHistorico>.IndexKeys
                        .Ascending(e => e.UsuarioId)
                        .Descending(e => e.Fecha),
                    new CreateIndexOptions { Name = "IX_usuarioId_fecha" }),

                new CreateIndexModel<EventoHistorico>(
                    Builders<EventoHistorico>.IndexKeys
                        .Ascending(e => e.TipoEvento)
                        .Descending(e => e.Fecha),
                    new CreateIndexOptions { Name = "IX_tipoEvento_fecha" }),

                new CreateIndexModel<EventoHistorico>(
                    Builders<EventoHistorico>.IndexKeys.Ascending(e => e.ExpireAt),
                    new CreateIndexOptions
                    {
                        Name = "IX_expireAt_ttl",
                        ExpireAfter = TimeSpan.Zero
                    })
            };

            await _collection.Indexes.CreateManyAsync(indexModels);
            _indicesInicializados = true;

            _logger.LogInformation("Índices de eventos históricos inicializados correctamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al inicializar índices de eventos históricos");
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}

