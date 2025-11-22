using GateKeep.Api.Application.Events;

namespace GateKeep.Api.Infrastructure.Events;

/// <summary>
/// Observer que registra todos los eventos en logs para auditoría y monitoreo
/// </summary>
public sealed class LoggingObserver : IEventObserver
{
    private readonly ILogger<LoggingObserver> _logger;

    public LoggingObserver(ILogger<LoggingObserver> logger)
    {
        _logger = logger;
    }

    public Task OnAccesoPermitidoAsync(long usuarioId, long espacioId, string puntoControl, DateTime fecha)
    {
        _logger.LogInformation(
            "Evento: Acceso Permitido - UsuarioId: {UsuarioId}, EspacioId: {EspacioId}, PuntoControl: {PuntoControl}, Fecha: {Fecha}",
            usuarioId, espacioId, puntoControl, fecha);
        return Task.CompletedTask;
    }

    public Task OnAccesoRechazadoAsync(long usuarioId, long? espacioId, string razon, string puntoControl, DateTime fecha)
    {
        _logger.LogWarning(
            "Evento: Acceso Rechazado - UsuarioId: {UsuarioId}, EspacioId: {EspacioId}, Razón: {Razon}, PuntoControl: {PuntoControl}, Fecha: {Fecha}",
            usuarioId, espacioId, razon, puntoControl, fecha);
        return Task.CompletedTask;
    }

    public Task OnCambioRolAsync(long usuarioId, string rolAnterior, string rolNuevo, long? modificadoPor, DateTime fecha)
    {
        _logger.LogInformation(
            "Evento: Cambio de Rol - UsuarioId: {UsuarioId}, RolAnterior: {RolAnterior}, RolNuevo: {RolNuevo}, ModificadoPor: {ModificadoPor}, Fecha: {Fecha}",
            usuarioId, rolAnterior, rolNuevo, modificadoPor, fecha);
        return Task.CompletedTask;
    }

    public Task OnUsuarioCreadoAsync(long usuarioId, string email, string nombre, string apellido, string rol, DateTime fecha)
    {
        _logger.LogInformation(
            "Evento: Usuario Creado - UsuarioId: {UsuarioId}, Email: {Email}, Nombre: {Nombre}, Apellido: {Apellido}, Rol: {Rol}, Fecha: {Fecha}",
            usuarioId, email, nombre, apellido, rol, fecha);
        return Task.CompletedTask;
    }

    public Task OnBeneficioAsignadoAsync(long usuarioId, long beneficioId, string beneficioNombre, DateTime fecha)
    {
        _logger.LogInformation(
            "Evento: Beneficio Asignado - UsuarioId: {UsuarioId}, BeneficioId: {BeneficioId}, BeneficioNombre: {BeneficioNombre}, Fecha: {Fecha}",
            usuarioId, beneficioId, beneficioNombre, fecha);
        return Task.CompletedTask;
    }

    public Task OnBeneficioDesasignadoAsync(long usuarioId, long beneficioId, string beneficioNombre, DateTime fecha)
    {
        _logger.LogInformation(
            "Evento: Beneficio Desasignado - UsuarioId: {UsuarioId}, BeneficioId: {BeneficioId}, BeneficioNombre: {BeneficioNombre}, Fecha: {Fecha}",
            usuarioId, beneficioId, beneficioNombre, fecha);
        return Task.CompletedTask;
    }
}

