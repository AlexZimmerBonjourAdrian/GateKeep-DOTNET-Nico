using GateKeep.Api.Application.Notificaciones;
using GateKeep.Api.Application.Usuarios;

namespace GateKeep.Api.Infrastructure.Notificaciones;

public sealed class NotificacionUsuarioValidationService : INotificacionUsuarioValidationService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly INotificacionRepository _notificacionRepository;

    public NotificacionUsuarioValidationService(
        IUsuarioRepository usuarioRepository,
        INotificacionRepository notificacionRepository)
    {
        _usuarioRepository = usuarioRepository;
        _notificacionRepository = notificacionRepository;
    }

    public async Task<bool> ValidarUsuarioExisteAsync(long usuarioId)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
        return usuario is not null;
    }

    public async Task ValidarIntegridadReferencialAsync(long usuarioId, string notificacionId)
    {
        // Validar que el usuario existe en PostgreSQL
        var usuarioExiste = await ValidarUsuarioExisteAsync(usuarioId);
        if (!usuarioExiste)
        {
            throw new InvalidOperationException($"El usuario con ID {usuarioId} no existe en PostgreSQL");
        }

        // Validar que la notificación existe en MongoDB
        var notificacion = await _notificacionRepository.ObtenerPorIdAsync(notificacionId);
        if (notificacion is null)
        {
            throw new InvalidOperationException($"La notificación con ID {notificacionId} no existe en MongoDB");
        }
    }
}

