using GateKeep.Api.Domain.Entities;

namespace GateKeep.Api.Application.Notificaciones;

public interface INotificacionTransactionService
{
    Task<NotificacionUsuario> CrearNotificacionUsuarioConCompensacionAsync(
        long usuarioId, 
        string notificacionId);
    
    Task<bool> EliminarNotificacionUsuarioConCompensacionAsync(
        long usuarioId, 
        string notificacionId);
}

