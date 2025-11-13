using GateKeep.Api.Application.Notificaciones;
using GateKeep.Api.Application.Queues;
using GateKeep.Api.Infrastructure.Notificaciones;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GateKeep.Api.Infrastructure.Queues;

/// <summary>
/// Servicio de background que procesa la cola de sincronizaciones pendientes
/// </summary>
public class SincronizacionQueueProcessor : BackgroundService
{
    private readonly ISincronizacionQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SincronizacionQueueProcessor> _logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(5);

    public SincronizacionQueueProcessor(
        ISincronizacionQueue queue,
        IServiceProvider serviceProvider,
        ILogger<SincronizacionQueueProcessor> logger)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SincronizacionQueueProcessor iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_queue.TryDequeue(out var item))
                {
                    await ProcessSincronizacionAsync(item.tipo, item.usuarioId, stoppingToken);
                }
                else
                {
                    await Task.Delay(_processingInterval, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando sincronización de la cola");
                await Task.Delay(_processingInterval, stoppingToken);
            }
        }

        _logger.LogInformation("SincronizacionQueueProcessor detenido");
    }

    private async Task ProcessSincronizacionAsync(string tipo, long usuarioId, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var sincronizacionService = scope.ServiceProvider.GetRequiredService<NotificacionSincronizacionService>();

        try
        {
            _logger.LogDebug("Procesando sincronización: Tipo={Tipo}, UsuarioId={UsuarioId}", tipo, usuarioId);

            switch (tipo)
            {
                case "notificaciones":
                    await sincronizacionService.ProcesarValidarConsistenciaAsync(usuarioId);
                    break;
                case "eliminacion_usuario":
                    await sincronizacionService.ProcesarSincronizarEliminacionUsuarioAsync(usuarioId);
                    break;
                case "limpiar_huerfanos":
                    await sincronizacionService.ProcesarLimpiarRegistrosHuerfanosAsync(usuarioId);
                    break;
                default:
                    _logger.LogWarning("Tipo de sincronización desconocido: {Tipo}", tipo);
                    break;
            }

            _logger.LogDebug("Sincronización completada: Tipo={Tipo}, UsuarioId={UsuarioId}", tipo, usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando sincronización: Tipo={Tipo}, UsuarioId={UsuarioId}", tipo, usuarioId);
            // Re-enqueue para reintento (opcional, podría implementar lógica de reintentos)
        }
    }
}

