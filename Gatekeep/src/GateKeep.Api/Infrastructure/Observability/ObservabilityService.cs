using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace GateKeep.Api.Infrastructure.Observability;

/// <summary>
/// Implementación del servicio de observabilidad para registrar métricas de negocio
/// </summary>
public class ObservabilityService : IObservabilityService
{
    private readonly Meter _meter;
    private readonly Counter<long> _accesosCounter;
    private readonly Counter<long> _eventosCounter;
    private readonly Counter<long> _notificacionesCounter;
    private readonly Counter<long> _erroresCounter;
    private readonly Counter<long> _cacheOperationsCounter;
    private readonly Histogram<double> _databaseOperationDuration;
    private readonly ILogger<ObservabilityService> _logger;

    public ObservabilityService(ILogger<ObservabilityService> logger)
    {
        _logger = logger;
        _meter = new Meter("GateKeep.Api", "1.0.0");

        // Contadores
        _accesosCounter = _meter.CreateCounter<long>(
            "gatekeep.accesos.total",
            unit: "accesos",
            description: "Número total de accesos al sistema");

        _eventosCounter = _meter.CreateCounter<long>(
            "gatekeep.eventos.total",
            unit: "eventos",
            description: "Número total de eventos creados");

        _notificacionesCounter = _meter.CreateCounter<long>(
            "gatekeep.notificaciones.total",
            unit: "notificaciones",
            description: "Número total de notificaciones enviadas");

        _erroresCounter = _meter.CreateCounter<long>(
            "gatekeep.errores.total",
            unit: "errores",
            description: "Número total de errores en el sistema");

        _cacheOperationsCounter = _meter.CreateCounter<long>(
            "gatekeep.cache.operations.total",
            unit: "operaciones",
            description: "Número total de operaciones de cache");

        // Histogramas
        _databaseOperationDuration = _meter.CreateHistogram<double>(
            "gatekeep.database.duration",
            unit: "ms",
            description: "Duración de operaciones de base de datos");
    }

    public void RecordAcceso(string tipoAcceso, bool exitoso)
    {
        var tags = new TagList
        {
            { "tipo", tipoAcceso },
            { "exitoso", exitoso.ToString().ToLower() }
        };

        _accesosCounter.Add(1, tags);
        
        _logger.LogInformation(
            "Acceso registrado: Tipo={TipoAcceso}, Exitoso={Exitoso}",
            tipoAcceso, exitoso);
    }

    public void RecordEventoCreado(string tipoEvento)
    {
        var tags = new TagList
        {
            { "tipo", tipoEvento }
        };

        _eventosCounter.Add(1, tags);
        
        _logger.LogInformation("Evento creado: Tipo={TipoEvento}", tipoEvento);
    }

    public void RecordNotificacionEnviada(string canal, bool exitosa)
    {
        var tags = new TagList
        {
            { "canal", canal },
            { "exitosa", exitosa.ToString().ToLower() }
        };

        _notificacionesCounter.Add(1, tags);
        
        _logger.LogInformation(
            "Notificación enviada: Canal={Canal}, Exitosa={Exitosa}",
            canal, exitosa);
    }

    public void RecordDatabaseOperation(string database, string operation, double durationMs)
    {
        var tags = new TagList
        {
            { "database", database },
            { "operation", operation }
        };

        _databaseOperationDuration.Record(durationMs, tags);
        
        if (durationMs > 1000)
        {
            _logger.LogWarning(
                "Operación de base de datos lenta: Database={Database}, Operation={Operation}, Duration={Duration}ms",
                database, operation, durationMs);
        }
    }

    public void RecordCacheOperation(string operation, bool hit)
    {
        var tags = new TagList
        {
            { "operation", operation },
            { "result", hit ? "hit" : "miss" }
        };

        _cacheOperationsCounter.Add(1, tags);
    }

    public void RecordError(string component, string errorType)
    {
        var tags = new TagList
        {
            { "component", component },
            { "error_type", errorType }
        };

        _erroresCounter.Add(1, tags);
        
        _logger.LogError(
            "Error registrado: Component={Component}, ErrorType={ErrorType}",
            component, errorType);
    }
}

