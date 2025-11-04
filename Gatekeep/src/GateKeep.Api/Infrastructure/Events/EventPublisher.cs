using GateKeep.Api.Application.Events;

namespace GateKeep.Api.Infrastructure.Events;

/// <summary>
/// Implementación del patrón Observer para publicar eventos
/// </summary>
public class EventPublisher : IEventPublisher
{
    private readonly List<IEventObserver> _observers = new();
    private readonly object _lock = new();

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
        IEventObserver[] observersCopy;
        lock (_lock)
        {
            observersCopy = _observers.ToArray();
        }

        var tasks = observersCopy.Select(observer =>
        {
            try
            {
                return observer.OnAccesoPermitidoAsync(usuarioId, espacioId, puntoControl, fecha);
            }
            catch (Exception ex)
            {
                // Log error pero no romper el flujo principal
                Console.WriteLine($"Error notificando acceso permitido a observer {observer.GetType().Name}: {ex.Message}");
                return Task.CompletedTask;
            }
        });

        await Task.WhenAll(tasks);
    }

    public async Task NotifyAccesoRechazadoAsync(long usuarioId, long? espacioId, string razon, string puntoControl, DateTime fecha)
    {
        IEventObserver[] observersCopy;
        lock (_lock)
        {
            observersCopy = _observers.ToArray();
        }

        var tasks = observersCopy.Select(observer =>
        {
            try
            {
                return observer.OnAccesoRechazadoAsync(usuarioId, espacioId, razon, puntoControl, fecha);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error notificando acceso rechazado a observer {observer.GetType().Name}: {ex.Message}");
                return Task.CompletedTask;
            }
        });

        await Task.WhenAll(tasks);
    }

    public async Task NotifyCambioRolAsync(long usuarioId, string rolAnterior, string rolNuevo, long? modificadoPor, DateTime fecha)
    {
        IEventObserver[] observersCopy;
        lock (_lock)
        {
            observersCopy = _observers.ToArray();
        }

        var tasks = observersCopy.Select(observer =>
        {
            try
            {
                return observer.OnCambioRolAsync(usuarioId, rolAnterior, rolNuevo, modificadoPor, fecha);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error notificando cambio de rol a observer {observer.GetType().Name}: {ex.Message}");
                return Task.CompletedTask;
            }
        });

        await Task.WhenAll(tasks);
    }

    public async Task NotifyUsuarioCreadoAsync(long usuarioId, string email, string nombre, string apellido, string rol, DateTime fecha)
    {
        IEventObserver[] observersCopy;
        lock (_lock)
        {
            observersCopy = _observers.ToArray();
        }

        var tasks = observersCopy.Select(observer =>
        {
            try
            {
                return observer.OnUsuarioCreadoAsync(usuarioId, email, nombre, apellido, rol, fecha);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error notificando usuario creado a observer {observer.GetType().Name}: {ex.Message}");
                return Task.CompletedTask;
            }
        });

        await Task.WhenAll(tasks);
    }

    public async Task NotifyBeneficioAsignadoAsync(long usuarioId, long beneficioId, string beneficioNombre, DateTime fecha)
    {
        IEventObserver[] observersCopy;
        lock (_lock)
        {
            observersCopy = _observers.ToArray();
        }

        var tasks = observersCopy.Select(observer =>
        {
            try
            {
                return observer.OnBeneficioAsignadoAsync(usuarioId, beneficioId, beneficioNombre, fecha);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error notificando beneficio asignado a observer {observer.GetType().Name}: {ex.Message}");
                return Task.CompletedTask;
            }
        });

        await Task.WhenAll(tasks);
    }

    public async Task NotifyBeneficioDesasignadoAsync(long usuarioId, long beneficioId, string beneficioNombre, DateTime fecha)
    {
        IEventObserver[] observersCopy;
        lock (_lock)
        {
            observersCopy = _observers.ToArray();
        }

        var tasks = observersCopy.Select(observer =>
        {
            try
            {
                return observer.OnBeneficioDesasignadoAsync(usuarioId, beneficioId, beneficioNombre, fecha);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error notificando beneficio desasignado a observer {observer.GetType().Name}: {ex.Message}");
                return Task.CompletedTask;
            }
        });

        await Task.WhenAll(tasks);
    }
}

