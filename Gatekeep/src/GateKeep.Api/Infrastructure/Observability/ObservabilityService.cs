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
    private readonly Gauge<long> _sincronizacionesEnProceso;
    private readonly Gauge<long> _eventosConError;
    private readonly Gauge<long> _sincronizacionesPendientes;
    private readonly Gauge<long> _eventosPendientes;
    private readonly ILogger<ObservabilityService> _logger;
    private readonly Dictionary<string, long> _sincronizacionesCount = new();
    private readonly Dictionary<string, long> _eventosErrorCount = new();
    private readonly Dictionary<string, long> _sincronizacionesPendientesCount = new();
    private readonly Dictionary<string, long> _eventosPendientesCount = new();
    private readonly object _lock = new();

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

        // Gauges
        _sincronizacionesEnProceso = _meter.CreateGauge<long>(
            "gatekeep.sincronizaciones.en_proceso",
            unit: "sincronizaciones",
            description: "Número de sincronizaciones actualmente en proceso");

        _eventosConError = _meter.CreateGauge<long>(
            "gatekeep.eventos.con_error",
            unit: "eventos",
            description: "Número de eventos que fallaron al procesarse");

        _sincronizacionesPendientes = _meter.CreateGauge<long>(
            "gatekeep.sincronizaciones.pendientes",
            unit: "sincronizaciones",
            description: "Número de sincronizaciones pendientes en la cola");

        _eventosPendientes = _meter.CreateGauge<long>(
            "gatekeep.eventos.pendientes",
            unit: "eventos",
            description: "Número de eventos pendientes en la cola");
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

    public void RecordSincronizacionIniciada(string tipo)
    {
        lock (_lock)
        {
            _sincronizacionesCount.TryGetValue(tipo, out var current);
            _sincronizacionesCount[tipo] = current + 1;
            
            var tags = new TagList { { "tipo", tipo } };
            _sincronizacionesEnProceso.Record(_sincronizacionesCount[tipo], tags);
        }
        
        _logger.LogDebug("Sincronización iniciada: Tipo={Tipo}", tipo);
    }

    public void RecordSincronizacionCompletada(string tipo)
    {
        lock (_lock)
        {
            if (_sincronizacionesCount.TryGetValue(tipo, out var current) && current > 0)
            {
                _sincronizacionesCount[tipo] = current - 1;
                
                var tags = new TagList { { "tipo", tipo } };
                _sincronizacionesEnProceso.Record(_sincronizacionesCount[tipo], tags);
            }
        }
        
        _logger.LogDebug("Sincronización completada: Tipo={Tipo}", tipo);
    }

    public void RecordEventoError(string tipoEvento)
    {
        lock (_lock)
        {
            _eventosErrorCount.TryGetValue(tipoEvento, out var current);
            _eventosErrorCount[tipoEvento] = current + 1;
            
            var tags = new TagList { { "tipo", tipoEvento } };
            _eventosConError.Record(_eventosErrorCount[tipoEvento], tags);
        }
        
        // También registrar como error general
        RecordError("EventPublisher", $"evento_{tipoEvento}_error");
        
        _logger.LogWarning("Evento con error al procesarse: Tipo={TipoEvento}", tipoEvento);
    }

    public void UpdateSincronizacionesPendientes(string tipo, int cantidad)
    {
        lock (_lock)
        {
            _sincronizacionesPendientesCount[tipo] = cantidad;
            
            var tags = new TagList { { "tipo", tipo } };
            _sincronizacionesPendientes.Record(cantidad, tags);
        }
    }

    public void UpdateEventosPendientes(string tipoEvento, int cantidad)
    {
        lock (_lock)
        {
            _eventosPendientesCount[tipoEvento] = cantidad;
            
            var tags = new TagList { { "tipo", tipoEvento } };
            _eventosPendientes.Record(cantidad, tags);
        }
    }
}

