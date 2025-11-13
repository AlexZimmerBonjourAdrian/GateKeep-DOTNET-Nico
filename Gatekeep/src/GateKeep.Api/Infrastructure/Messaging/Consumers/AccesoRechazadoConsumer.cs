using GateKeep.Api.Domain.Events;
using GateKeep.Api.Application.Notificaciones;
using GateKeep.Api.Infrastructure.Observability;
using MassTransit;

namespace GateKeep.Api.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumidor de eventos de acceso rechazado
/// Procesa mensajes de forma asíncrona con idempotencia y manejo de errores
/// </summary>
public class AccesoRechazadoConsumer : IConsumer<AccesoRechazadoEvent>
{
    private readonly ILogger<AccesoRechazadoConsumer> _logger;
    private readonly IIdempotencyService _idempotencyService;
    private readonly INotificacionService _notificacionService;
    private readonly IObservabilityService _observabilityService;

    public AccesoRechazadoConsumer(
        ILogger<AccesoRechazadoConsumer> logger,
        IIdempotencyService idempotencyService,
        INotificacionService notificacionService,
        IObservabilityService observabilityService)
    {
        _logger = logger;
        _idempotencyService = idempotencyService;
        _notificacionService = notificacionService;
        _observabilityService = observabilityService;
    }

    public async Task Consume(ConsumeContext<AccesoRechazadoEvent> context)
    {
        var message = context.Message;
        var messageId = context.MessageId?.ToString() ?? message.EventId.ToString();

        _logger.LogInformation(
            "Procesando AccesoRechazadoEvent - EventId: {EventId}, UsuarioId: {UsuarioId}, EspacioId: {EspacioId}, IdempotencyKey: {IdempotencyKey}",
            message.EventId, message.UsuarioId, message.EspacioId, message.IdempotencyKey);

        try
        {
            // Verificar idempotencia
            var yaProcessado = await _idempotencyService.IsProcessedAsync(message.IdempotencyKey, context.CancellationToken);
            if (yaProcessado)
            {
                _logger.LogWarning(
                    "Mensaje duplicado detectado - IdempotencyKey: {IdempotencyKey}, EventId: {EventId}. Ignorando.",
                    message.IdempotencyKey, message.EventId);
                
                return; // Mensaje ya procesado, no hacer nada
            }

            // Procesar el mensaje
            await ProcesarAccesoRechazadoAsync(message, context.CancellationToken);

            // Marcar como procesado
            await _idempotencyService.MarkAsProcessedAsync(message.IdempotencyKey, cancellationToken: context.CancellationToken);

            _logger.LogInformation(
                "AccesoRechazadoEvent procesado exitosamente - EventId: {EventId}, IdempotencyKey: {IdempotencyKey}",
                message.EventId, message.IdempotencyKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error procesando AccesoRechazadoEvent - EventId: {EventId}, IdempotencyKey: {IdempotencyKey}",
                message.EventId, message.IdempotencyKey);

            _observabilityService.RecordError("RabbitMQ.Consumer", ex.GetType().Name);

            // Re-lanzar para que MassTransit maneje el reintento
            throw;
        }
    }

    private async Task ProcesarAccesoRechazadoAsync(AccesoRechazadoEvent evento, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Iniciando procesamiento de acceso rechazado - Usuario: {UsuarioId}, Espacio: {EspacioId}, Razón: {Razon}",
            evento.UsuarioId, evento.EspacioId, evento.Razon);

        // 1. Crear notificación para el usuario
        try
        {
            var mensaje = $"Tu acceso fue rechazado en {evento.PuntoControl}. Motivo: {evento.Razon}";
            
            await _notificacionService.CrearNotificacionAsync(
                mensaje,
                "Acceso",
                evento.UsuarioId);

            _logger.LogInformation(
                "Notificación creada para usuario {UsuarioId} por acceso rechazado",
                evento.UsuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error creando notificación para usuario {UsuarioId}", 
                evento.UsuarioId);
            // No re-lanzar, continuar con otras tareas
        }

        // 2. Registrar en el sistema de auditoría/analytics
        try
        {
            _logger.LogWarning(
                "Acceso rechazado registrado - Usuario: {UsuarioId}, Espacio: {EspacioId}, Punto: {PuntoControl}, Tipo: {TipoError}, Timestamp: {Timestamp}",
                evento.UsuarioId, evento.EspacioId, evento.PuntoControl, evento.TipoError, evento.Timestamp);

            // Registrar métrica de acceso rechazado procesado
            _observabilityService.RecordAcceso("RechazadoAsync", false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registrando en auditoría");
        }

        // 3. Verificar si hay múltiples rechazos y tomar acciones
        // (por ejemplo, bloquear temporalmente, alertar a seguridad, etc.)
        try
        {
            await VerificarMultiplesRechazosAsync(evento, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando múltiples rechazos");
        }

        _logger.LogInformation(
            "Procesamiento completado para acceso rechazado - EventId: {EventId}",
            evento.EventId);
    }

    private async Task VerificarMultiplesRechazosAsync(AccesoRechazadoEvent evento, CancellationToken cancellationToken)
    {
        // Aquí podrías implementar lógica para:
        // - Contar rechazos recientes del usuario
        // - Alertar a seguridad si hay muchos intentos fallidos
        // - Bloquear temporalmente al usuario si es necesario
        
        _logger.LogDebug(
            "Verificando múltiples rechazos para usuario {UsuarioId}",
            evento.UsuarioId);

        // Placeholder para lógica futura
        await Task.CompletedTask;
    }
}
