using GateKeep.Api.Domain.Events;
using GateKeep.Api.Application.Notificaciones;
using GateKeep.Api.Infrastructure.Observability;
using MassTransit;

namespace GateKeep.Api.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumidor de eventos de beneficio canjeado
/// Procesa mensajes de forma asíncrona con idempotencia
/// </summary>
public class BeneficioCanjeadoConsumer : IConsumer<BeneficioCanjeadoEvent>
{
    private readonly ILogger<BeneficioCanjeadoConsumer> _logger;
    private readonly IIdempotencyService _idempotencyService;
    private readonly INotificacionService _notificacionService;
    private readonly IObservabilityService _observabilityService;

    public BeneficioCanjeadoConsumer(
        ILogger<BeneficioCanjeadoConsumer> logger,
        IIdempotencyService idempotencyService,
        INotificacionService notificacionService,
        IObservabilityService observabilityService)
    {
        _logger = logger;
        _idempotencyService = idempotencyService;
        _notificacionService = notificacionService;
        _observabilityService = observabilityService;
    }

    public async Task Consume(ConsumeContext<BeneficioCanjeadoEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Procesando BeneficioCanjeadoEvent - EventId: {EventId}, UsuarioId: {UsuarioId}, BeneficioId: {BeneficioId}",
            message.EventId, message.UsuarioId, message.BeneficioId);

        try
        {
            // Verificar idempotencia
            var yaProcessado = await _idempotencyService.IsProcessedAsync(message.IdempotencyKey, context.CancellationToken);
            if (yaProcessado)
            {
                _logger.LogWarning(
                    "Mensaje duplicado detectado - IdempotencyKey: {IdempotencyKey}. Ignorando.",
                    message.IdempotencyKey);
                return;
            }

            // Procesar el mensaje
            await ProcesarBeneficioCanjeadoAsync(message, context.CancellationToken);

            // Marcar como procesado
            await _idempotencyService.MarkAsProcessedAsync(message.IdempotencyKey, cancellationToken: context.CancellationToken);

            _logger.LogInformation(
                "BeneficioCanjeadoEvent procesado exitosamente - EventId: {EventId}",
                message.EventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error procesando BeneficioCanjeadoEvent - EventId: {EventId}",
                message.EventId);

            _observabilityService.RecordError("RabbitMQ.Consumer", ex.GetType().Name);
            throw;
        }
    }

    private async Task ProcesarBeneficioCanjeadoAsync(BeneficioCanjeadoEvent evento, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Procesando canje de beneficio - Usuario: {UsuarioId}, Beneficio: {BeneficioId} ({NombreBeneficio}), Puntos: {Puntos}",
            evento.UsuarioId, evento.BeneficioId, evento.NombreBeneficio, evento.PuntosCanjeados);

        // 1. Crear notificación de confirmación
        try
        {
            var mensaje = $"¡Has canjeado exitosamente el beneficio '{evento.NombreBeneficio}'! Puntos utilizados: {evento.PuntosCanjeados}";
            
            await _notificacionService.CrearNotificacionAsync(
                mensaje,
                "Beneficio",
                evento.UsuarioId);

            _logger.LogInformation(
                "Notificación de canje enviada a usuario {UsuarioId}",
                evento.UsuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error enviando notificación de canje a usuario {UsuarioId}", 
                evento.UsuarioId);
        }

        // 2. Registrar métrica
        try
        {
            _observabilityService.RecordAcceso("BeneficioCanjeado", true);
            
            _logger.LogInformation(
                "Beneficio canjeado registrado - Usuario: {UsuarioId}, Beneficio: {BeneficioId}, Puntos: {Puntos}",
                evento.UsuarioId, evento.BeneficioId, evento.PuntosCanjeados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registrando métrica de canje");
        }
    }
}
