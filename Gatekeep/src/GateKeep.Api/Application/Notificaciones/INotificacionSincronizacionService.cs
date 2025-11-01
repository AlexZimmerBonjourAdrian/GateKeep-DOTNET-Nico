namespace GateKeep.Api.Application.Notificaciones;

public interface INotificacionSincronizacionService
{
    Task LimpiarRegistrosHuerfanosAsync(long usuarioId);
    Task ValidarConsistenciaAsync(long usuarioId);
    Task SincronizarEliminacionUsuarioAsync(long usuarioId);
}

