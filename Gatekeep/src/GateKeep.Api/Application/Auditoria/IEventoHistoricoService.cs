namespace GateKeep.Api.Application.Auditoria;

public interface IEventoHistoricoService
{
    Task RegistrarAccesoAsync(
        long usuarioId,
        long espacioId,
        string resultado,
        string puntoControl,
        Dictionary<string, object>? datosAdicionales = null);
    Task RegistrarRechazoAsync(
        long usuarioId,
        long? espacioId,
        string razon,
        string puntoControl,
        Dictionary<string, object>? datosAdicionales = null);
    Task RegistrarNotificacionAsync(
        long usuarioId,
        string tipoNotificacion,
        string resultado,
        Dictionary<string, object>? datosAdicionales = null);
    Task RegistrarCambioRolAsync(
        long usuarioId,
        string rolAnterior,
        string rolNuevo,
        long? modificadoPor,
        Dictionary<string, object>? datosAdicionales = null);
}

