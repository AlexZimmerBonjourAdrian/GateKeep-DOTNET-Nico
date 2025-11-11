using GateKeep.Api.Application.Queues;
using GateKeep.Api.Infrastructure.Observability;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GateKeep.Api.Infrastructure.Queues;

/// <summary>
/// Servicio de background que actualiza las métricas de backlog pendiente periódicamente
/// </summary>
public class BacklogMetricsUpdater : BackgroundService
{
    private readonly ISincronizacionQueue _sincronizacionQueue;
    private readonly IEventoQueue _eventoQueue;
    private readonly IObservabilityService _observabilityService;
    private readonly ILogger<BacklogMetricsUpdater> _logger;
    private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(10);

    public BacklogMetricsUpdater(
        ISincronizacionQueue sincronizacionQueue,
        IEventoQueue eventoQueue,
        IObservabilityService observabilityService,
        ILogger<BacklogMetricsUpdater> logger)
    {
        _sincronizacionQueue = sincronizacionQueue;
        _eventoQueue = eventoQueue;
        _observabilityService = observabilityService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BacklogMetricsUpdater iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                UpdateSincronizacionesMetrics();
                UpdateEventosMetrics();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando métricas de backlog");
            }

            await Task.Delay(_updateInterval, stoppingToken);
        }

        _logger.LogInformation("BacklogMetricsUpdater detenido");
    }

    private void UpdateSincronizacionesMetrics()
    {
        var tipos = new[] { "notificaciones", "eliminacion_usuario", "limpiar_huerfanos" };
        
        foreach (var tipo in tipos)
        {
            var cantidad = _sincronizacionQueue.CountByTipo(tipo);
            _observabilityService.UpdateSincronizacionesPendientes(tipo, cantidad);
        }
    }

    private void UpdateEventosMetrics()
    {
        var tipos = new[] { 
            "acceso_permitido", 
            "acceso_rechazado", 
            "cambio_rol", 
            "usuario_creado", 
            "beneficio_asignado", 
            "beneficio_desasignado" 
        };
        
        foreach (var tipo in tipos)
        {
            var cantidad = _eventoQueue.CountByTipo(tipo);
            _observabilityService.UpdateEventosPendientes(tipo, cantidad);
        }
    }
}

