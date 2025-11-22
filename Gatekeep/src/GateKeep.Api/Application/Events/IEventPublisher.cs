namespace GateKeep.Api.Application.Events;

/// <summary>
/// Interfaz para publicar eventos a los observers registrados
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Registra un observer para recibir notificaciones de eventos
    /// </summary>
    void Subscribe(IEventObserver observer);

    /// <summary>
    /// Elimina un observer de la lista de suscriptores
    /// </summary>
    void Unsubscribe(IEventObserver observer);

    /// <summary>
    /// Notifica a todos los observers sobre un acceso permitido
    /// </summary>
    Task NotifyAccesoPermitidoAsync(long usuarioId, long espacioId, string puntoControl, DateTime fecha);

    /// <summary>
    /// Notifica a todos los observers sobre un acceso rechazado
    /// </summary>
    Task NotifyAccesoRechazadoAsync(long usuarioId, long? espacioId, string razon, string puntoControl, DateTime fecha);

    /// <summary>
    /// Notifica a todos los observers sobre un cambio de rol
    /// </summary>
    Task NotifyCambioRolAsync(long usuarioId, string rolAnterior, string rolNuevo, long? modificadoPor, DateTime fecha);

    /// <summary>
    /// Notifica a todos los observers sobre la creación de un usuario
    /// </summary>
    Task NotifyUsuarioCreadoAsync(long usuarioId, string email, string nombre, string apellido, string rol, DateTime fecha);

    /// <summary>
    /// Notifica a todos los observers sobre la asignación de un beneficio
    /// </summary>
    Task NotifyBeneficioAsignadoAsync(long usuarioId, long beneficioId, string beneficioNombre, DateTime fecha);

    /// <summary>
    /// Notifica a todos los observers sobre la desasignación de un beneficio
    /// </summary>
    Task NotifyBeneficioDesasignadoAsync(long usuarioId, long beneficioId, string beneficioNombre, DateTime fecha);
}

