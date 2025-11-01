namespace GateKeep.Api.Application.Notificaciones;

public interface INotificacionUsuarioValidationService
{
    Task<bool> ValidarUsuarioExisteAsync(long usuarioId);
    Task ValidarIntegridadReferencialAsync(long usuarioId, string notificacionId);
}

