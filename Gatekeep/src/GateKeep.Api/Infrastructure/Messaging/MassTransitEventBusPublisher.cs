using GateKeep.Api.Domain.Events;
using MassTransit;

namespace GateKeep.Api.Infrastructure.Messaging;

/// <summary>
/// Servicio para publicar eventos de dominio a RabbitMQ
/// </summary>
public interface IEventBusPublisher
{
    Task PublishAccesoRechazadoAsync(
        long usuarioId,
        long? espacioId,
        string razon,
        string puntoControl,
        string tipoError,
        Dictionary<string, object>? detallesAdicionales = null,
        CancellationToken cancellationToken = default);

    Task PublishBeneficioCanjeadoAsync(
        long usuarioId,
        long beneficioId,
        string nombreBeneficio,
        string puntoControl,
        int puntosCanjeados,
        CancellationToken cancellationToken = default);
}

public class MassTransitEventBusPublisher : IEventBusPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<MassTransitEventBusPublisher> _logger;

    public MassTransitEventBusPublisher(
        IPublishEndpoint publishEndpoint,
        ILogger<MassTransitEventBusPublisher> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task PublishAccesoRechazadoAsync(
        long usuarioId,
        long? espacioId,
        string razon,
        string puntoControl,
        string tipoError,
        Dictionary<string, object>? detallesAdicionales = null,
        CancellationToken cancellationToken = default)
    {
        var evento = new AccesoRechazadoEvent(
            usuarioId,
            espacioId,
            razon,
            puntoControl,
            tipoError,
            detallesAdicionales);

        try
        {
            await _publishEndpoint.Publish(evento, cancellationToken);

            _logger.LogInformation(
                "Evento AccesoRechazado publicado - EventId: {EventId}, UsuarioId: {UsuarioId}, EspacioId: {EspacioId}",
                evento.EventId, usuarioId, espacioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error publicando AccesoRechazadoEvent - UsuarioId: {UsuarioId}, EspacioId: {EspacioId}",
                usuarioId, espacioId);
            
            // No re-lanzar para no afectar el flujo principal
            // El rechazo ya fue registrado en BD, la notificación asíncrona es secundaria
        }
    }

    public async Task PublishBeneficioCanjeadoAsync(
        long usuarioId,
        long beneficioId,
        string nombreBeneficio,
        string puntoControl,
        int puntosCanjeados,
        CancellationToken cancellationToken = default)
    {
        var evento = new BeneficioCanjeadoEvent(
            usuarioId,
            beneficioId,
            nombreBeneficio,
            puntoControl,
            puntosCanjeados);

        try
        {
            await _publishEndpoint.Publish(evento, cancellationToken);

            _logger.LogInformation(
                "Evento BeneficioCanjeado publicado - EventId: {EventId}, UsuarioId: {UsuarioId}, BeneficioId: {BeneficioId}",
                evento.EventId, usuarioId, beneficioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error publicando BeneficioCanjeadoEvent - UsuarioId: {UsuarioId}, BeneficioId: {BeneficioId}",
                usuarioId, beneficioId);
        }
    }
}
