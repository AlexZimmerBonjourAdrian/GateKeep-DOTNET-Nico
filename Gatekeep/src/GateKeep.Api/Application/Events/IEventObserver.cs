namespace GateKeep.Api.Application.Events;

/// <summary>
/// Interfaz base para observers que escuchan eventos del sistema
/// </summary>
public interface IEventObserver
{
    /// <summary>
    /// Notifica cuando se permite un acceso
    /// </summary>
    Task OnAccesoPermitidoAsync(long usuarioId, long espacioId, string puntoControl, DateTime fecha);

    /// <summary>
    /// Notifica cuando se rechaza un acceso
    /// </summary>
    Task OnAccesoRechazadoAsync(long usuarioId, long? espacioId, string razon, string puntoControl, DateTime fecha);

    /// <summary>
    /// Notifica cuando se cambia el rol de un usuario
    /// </summary>
    Task OnCambioRolAsync(long usuarioId, string rolAnterior, string rolNuevo, long? modificadoPor, DateTime fecha);

    /// <summary>
    /// Notifica cuando se crea un nuevo usuario
    /// </summary>
    Task OnUsuarioCreadoAsync(long usuarioId, string email, string nombre, string apellido, string rol, DateTime fecha);

    /// <summary>
    /// Notifica cuando se asigna un beneficio a un usuario
    /// </summary>
    Task OnBeneficioAsignadoAsync(long usuarioId, long beneficioId, string beneficioNombre, DateTime fecha);

    /// <summary>
    /// Notifica cuando se desasigna un beneficio de un usuario
    /// </summary>
    Task OnBeneficioDesasignadoAsync(long usuarioId, long beneficioId, string beneficioNombre, DateTime fecha);

    /// <summary>
    /// Notifica cuando se canjea un beneficio
    /// </summary>
    Task OnBeneficioCanjeadoAsync(long usuarioId, long beneficioId, string beneficioNombre, string puntoControl, DateTime fecha);
}

