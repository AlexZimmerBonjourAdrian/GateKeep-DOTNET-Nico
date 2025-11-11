using GateKeep.Api.Application.Events;
using GateKeep.Api.Application.Queues;
using GateKeep.Api.Infrastructure.Observability;
using GateKeep.Api.Infrastructure.Queues;
using Microsoft.Extensions.Logging;

namespace GateKeep.Api.Infrastructure.Events;

/// <summary>
/// Implementación del patrón Observer para publicar eventos
/// </summary>
public class EventPublisher : IEventPublisher
{
    private readonly List<IEventObserver> _observers = new();
    private readonly object _lock = new();
    private readonly IObservabilityService _observabilityService;
    private readonly IEventoQueue _eventoQueue;
    private readonly ILogger<EventPublisher> _logger;

    public EventPublisher(
        IObservabilityService observabilityService, 
        IEventoQueue eventoQueue,
        ILogger<EventPublisher> logger)
    {
        _observabilityService = observabilityService;
        _eventoQueue = eventoQueue;
        _logger = logger;
    }

    public void Subscribe(IEventObserver observer)
    {
        if (observer == null)
            throw new ArgumentNullException(nameof(observer));

        lock (_lock)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
        }
    }

    public void Unsubscribe(IEventObserver observer)
    {
        if (observer == null)
            return;

        lock (_lock)
        {
            _observers.Remove(observer);
        }
    }

    public async Task NotifyAccesoPermitidoAsync(long usuarioId, long espacioId, string puntoControl, DateTime fecha)
    {
        // Encolar en lugar de procesar directamente
        var eventoData = new EventoQueueProcessor.AccesoPermitidoData(usuarioId, espacioId, puntoControl, fecha);
        _eventoQueue.Enqueue("acceso_permitido", eventoData);
        await Task.CompletedTask;
    }

    public async Task NotifyAccesoRechazadoAsync(long usuarioId, long? espacioId, string razon, string puntoControl, DateTime fecha)
    {
        // Encolar en lugar de procesar directamente
        var eventoData = new EventoQueueProcessor.AccesoRechazadoData(usuarioId, espacioId, razon, puntoControl, fecha);
        _eventoQueue.Enqueue("acceso_rechazado", eventoData);
        await Task.CompletedTask;
    }

    public async Task NotifyCambioRolAsync(long usuarioId, string rolAnterior, string rolNuevo, long? modificadoPor, DateTime fecha)
    {
        // Encolar en lugar de procesar directamente
        var eventoData = new EventoQueueProcessor.CambioRolData(usuarioId, rolAnterior, rolNuevo, modificadoPor, fecha);
        _eventoQueue.Enqueue("cambio_rol", eventoData);
        await Task.CompletedTask;
    }

    public async Task NotifyUsuarioCreadoAsync(long usuarioId, string email, string nombre, string apellido, string rol, DateTime fecha)
    {
        // Encolar en lugar de procesar directamente
        var eventoData = new EventoQueueProcessor.UsuarioCreadoData(usuarioId, email, nombre, apellido, rol, fecha);
        _eventoQueue.Enqueue("usuario_creado", eventoData);
        await Task.CompletedTask;
    }

    public async Task NotifyBeneficioAsignadoAsync(long usuarioId, long beneficioId, string beneficioNombre, DateTime fecha)
    {
        // Encolar en lugar de procesar directamente
        var eventoData = new EventoQueueProcessor.BeneficioAsignadoData(usuarioId, beneficioId, beneficioNombre, fecha);
        _eventoQueue.Enqueue("beneficio_asignado", eventoData);
        await Task.CompletedTask;
    }

    public async Task NotifyBeneficioDesasignadoAsync(long usuarioId, long beneficioId, string beneficioNombre, DateTime fecha)
    {
        // Encolar en lugar de procesar directamente
        var eventoData = new EventoQueueProcessor.BeneficioDesasignadoData(usuarioId, beneficioId, beneficioNombre, fecha);
        _eventoQueue.Enqueue("beneficio_desasignado", eventoData);
        await Task.CompletedTask;
    }

    // Método interno para procesar eventos desde la cola (llamado por el procesador)
    internal async Task ProcesarEventoAsync(string tipoEvento, object eventoData)
    {
        IEventObserver[] observersCopy;
        lock (_lock)
        {
            observersCopy = _observers.ToArray();
        }

        var tasks = observersCopy.Select(async observer =>
        {
            try
            {
                switch (tipoEvento)
                {
                    case "acceso_permitido":
                        if (eventoData is EventoQueueProcessor.AccesoPermitidoData data1)
                        {
                            await observer.OnAccesoPermitidoAsync(data1.UsuarioId, data1.EspacioId, data1.PuntoControl, data1.Fecha);
                        }
                        break;
                    case "acceso_rechazado":
                        if (eventoData is EventoQueueProcessor.AccesoRechazadoData data2)
                        {
                            await observer.OnAccesoRechazadoAsync(data2.UsuarioId, data2.EspacioId, data2.Razon, data2.PuntoControl, data2.Fecha);
                        }
                        break;
                    case "cambio_rol":
                        if (eventoData is EventoQueueProcessor.CambioRolData data3)
                        {
                            await observer.OnCambioRolAsync(data3.UsuarioId, data3.RolAnterior, data3.RolNuevo, data3.ModificadoPor, data3.Fecha);
                        }
                        break;
                    case "usuario_creado":
                        if (eventoData is EventoQueueProcessor.UsuarioCreadoData data4)
                        {
                            await observer.OnUsuarioCreadoAsync(data4.UsuarioId, data4.Email, data4.Nombre, data4.Apellido, data4.Rol, data4.Fecha);
                        }
                        break;
                    case "beneficio_asignado":
                        if (eventoData is EventoQueueProcessor.BeneficioAsignadoData data5)
                        {
                            await observer.OnBeneficioAsignadoAsync(data5.UsuarioId, data5.BeneficioId, data5.BeneficioNombre, data5.Fecha);
                        }
                        break;
                    case "beneficio_desasignado":
                        if (eventoData is EventoQueueProcessor.BeneficioDesasignadoData data6)
                        {
                            await observer.OnBeneficioDesasignadoAsync(data6.UsuarioId, data6.BeneficioId, data6.BeneficioNombre, data6.Fecha);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                _observabilityService.RecordEventoError(tipoEvento);
                _logger.LogError(ex, "Error procesando evento {TipoEvento} en observer {ObserverType}", tipoEvento, observer.GetType().Name);
            }
        });

        await Task.WhenAll(tasks);
    }
}
