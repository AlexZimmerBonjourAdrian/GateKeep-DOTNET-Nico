using GateKeep.Api.Application.Events;
using GateKeep.Api.Application.Notificaciones;
using GateKeep.Api.Application.Espacios;

namespace GateKeep.Api.Infrastructure.Notificaciones;

/// <summary>
/// Observer que crea notificaciones automáticamente cuando ocurren eventos en el sistema
/// </summary>
public sealed class NotificacionObserver : IEventObserver
{
    private readonly INotificacionService _notificacionService;
    private readonly INotificacionTransactionService _transactionService;
    private readonly IEspacioRepository _espacioRepository;

    public NotificacionObserver(
        INotificacionService notificacionService,
        INotificacionTransactionService transactionService,
        IEspacioRepository espacioRepository)
    {
        _notificacionService = notificacionService;
        _transactionService = transactionService;
        _espacioRepository = espacioRepository;
    }

    public async Task OnAccesoPermitidoAsync(long usuarioId, long espacioId, string puntoControl, DateTime fecha)
    {
        try
        {
            var espacio = await _espacioRepository.ObtenerPorIdAsync(espacioId);
            var nombreEspacio = espacio?.Nombre ?? $"Espacio {espacioId}";
            var mensaje = $"Acceso permitido a {nombreEspacio} en el punto de control {puntoControl}";

            // Crear notificación
            var notificacion = await _notificacionService.CrearNotificacionAsync(
                mensaje,
                "acceso_permitido",
                usuarioId);

            // Asignar notificación al usuario
            await _transactionService.CrearNotificacionUsuarioConCompensacionAsync(
                usuarioId,
                notificacion.Id);
        }
        catch (Exception ex)
        {
            // Log error pero no romper el flujo principal
            Console.WriteLine($"Error creando notificación de acceso permitido: {ex.Message}");
        }
    }

    public async Task OnAccesoRechazadoAsync(long usuarioId, long? espacioId, string razon, string puntoControl, DateTime fecha)
    {
        try
        {
            string nombreEspacio = "espacio desconocido";
            if (espacioId.HasValue)
            {
                var espacio = await _espacioRepository.ObtenerPorIdAsync(espacioId.Value);
                nombreEspacio = espacio?.Nombre ?? $"Espacio {espacioId.Value}";
            }

            var mensaje = $"Acceso rechazado a {nombreEspacio} en {puntoControl}. Razón: {razon}";

            // Crear notificación
            var notificacion = await _notificacionService.CrearNotificacionAsync(
                mensaje,
                "acceso_rechazado",
                usuarioId);

            // Asignar notificación al usuario
            await _transactionService.CrearNotificacionUsuarioConCompensacionAsync(
                usuarioId,
                notificacion.Id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creando notificación de acceso rechazado: {ex.Message}");
        }
    }

    public async Task OnCambioRolAsync(long usuarioId, string rolAnterior, string rolNuevo, long? modificadoPor, DateTime fecha)
    {
        try
        {
            var mensaje = $"Tu rol ha sido cambiado de {rolAnterior} a {rolNuevo}";

            // Crear notificación
            var notificacion = await _notificacionService.CrearNotificacionAsync(
                mensaje,
                "cambio_rol",
                modificadoPor);

            // Asignar notificación al usuario afectado
            await _transactionService.CrearNotificacionUsuarioConCompensacionAsync(
                usuarioId,
                notificacion.Id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creando notificación de cambio de rol: {ex.Message}");
        }
    }

    public async Task OnUsuarioCreadoAsync(long usuarioId, string email, string nombre, string apellido, string rol, DateTime fecha)
    {
        try
        {
            var mensaje = $"Bienvenido a GateKeep. Tu cuenta ha sido creada exitosamente con el rol: {rol}";

            // Crear notificación
            var notificacion = await _notificacionService.CrearNotificacionAsync(
                mensaje,
                "usuario_creado",
                usuarioId);

            // Asignar notificación al usuario recién creado
            await _transactionService.CrearNotificacionUsuarioConCompensacionAsync(
                usuarioId,
                notificacion.Id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creando notificación de usuario creado: {ex.Message}");
        }
    }

    public async Task OnBeneficioAsignadoAsync(long usuarioId, long beneficioId, string beneficioNombre, DateTime fecha)
    {
        try
        {
            var mensaje = $"Se te ha asignado el beneficio: {beneficioNombre}";

            // Crear notificación
            var notificacion = await _notificacionService.CrearNotificacionAsync(
                mensaje,
                "beneficio_asignado",
                usuarioId);

            // Asignar notificación al usuario
            await _transactionService.CrearNotificacionUsuarioConCompensacionAsync(
                usuarioId,
                notificacion.Id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creando notificación de beneficio asignado: {ex.Message}");
        }
    }

    public async Task OnBeneficioDesasignadoAsync(long usuarioId, long beneficioId, string beneficioNombre, DateTime fecha)
    {
        try
        {
            var mensaje = $"El beneficio {beneficioNombre} ha sido desasignado de tu cuenta";

            // Crear notificación
            var notificacion = await _notificacionService.CrearNotificacionAsync(
                mensaje,
                "beneficio_desasignado",
                usuarioId);

            // Asignar notificación al usuario
            await _transactionService.CrearNotificacionUsuarioConCompensacionAsync(
                usuarioId,
                notificacion.Id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creando notificación de beneficio desasignado: {ex.Message}");
        }
    }
}

