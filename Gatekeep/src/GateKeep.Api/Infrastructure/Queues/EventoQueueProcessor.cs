using GateKeep.Api.Application.Events;
using GateKeep.Api.Application.Queues;
using GateKeep.Api.Infrastructure.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GateKeep.Api.Infrastructure.Queues;

/// <summary>
/// Servicio de background que procesa la cola de eventos pendientes
/// </summary>
public class EventoQueueProcessor : BackgroundService
{
    private readonly IEventoQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventoQueueProcessor> _logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(2);

    public EventoQueueProcessor(
        IEventoQueue queue,
        IServiceProvider serviceProvider,
        ILogger<EventoQueueProcessor> logger)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EventoQueueProcessor iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_queue.TryDequeue(out var item))
                {
                    await ProcessEventoAsync(item.tipoEvento, item.eventoData, stoppingToken);
                }
                else
                {
                    await Task.Delay(_processingInterval, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando evento de la cola");
                await Task.Delay(_processingInterval, stoppingToken);
            }
        }

        _logger.LogInformation("EventoQueueProcessor detenido");
    }

    private async Task ProcessEventoAsync(string tipoEvento, object eventoData, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var eventPublisher = scope.ServiceProvider.GetRequiredService<EventPublisher>();

        try
        {
            _logger.LogDebug("Procesando evento: Tipo={TipoEvento}", tipoEvento);

            await eventPublisher.ProcesarEventoAsync(tipoEvento, eventoData);

            _logger.LogDebug("Evento procesado: Tipo={TipoEvento}", tipoEvento);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando evento: Tipo={TipoEvento}", tipoEvento);
            // Re-enqueue para reintento (opcional)
        }
    }

    // Clases de datos para los eventos
    public record AccesoPermitidoData(long UsuarioId, long EspacioId, string PuntoControl, DateTime Fecha);
    public record AccesoRechazadoData(long UsuarioId, long? EspacioId, string Razon, string PuntoControl, DateTime Fecha);
    public record CambioRolData(long UsuarioId, string RolAnterior, string RolNuevo, long? ModificadoPor, DateTime Fecha);
    public record UsuarioCreadoData(long UsuarioId, string Email, string Nombre, string Apellido, string Rol, DateTime Fecha);
    public record BeneficioAsignadoData(long UsuarioId, long BeneficioId, string BeneficioNombre, DateTime Fecha);
    public record BeneficioDesasignadoData(long UsuarioId, long BeneficioId, string BeneficioNombre, DateTime Fecha);
    public record BeneficioCanjeadoData(long UsuarioId, long BeneficioId, string BeneficioNombre, string PuntoControl, DateTime Fecha);
}

