using GateKeep.Api.Application.Notificaciones;
using GateKeep.Api.Application.Usuarios;

namespace GateKeep.Api.Infrastructure.Usuarios;

public sealed class UsuarioSincronizacionService : IUsuarioSincronizacionService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly INotificacionSincronizacionService _notificacionSincronizacionService;

    public UsuarioSincronizacionService(
        IUsuarioRepository usuarioRepository,
        INotificacionSincronizacionService notificacionSincronizacionService)
    {
        _usuarioRepository = usuarioRepository;
        _notificacionSincronizacionService = notificacionSincronizacionService;
    }

    public async Task EliminarUsuarioCompletoAsync(long usuarioId)
    {
        // 1. Verificar que el usuario existe
        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
        if (usuario is null)
        {
            throw new InvalidOperationException($"El usuario con ID {usuarioId} no existe");
        }

        // 2. Eliminar usuario en PostgreSQL
        await _usuarioRepository.DeleteAsync(usuarioId);

        // 3. Sincronizar eliminación en MongoDB (limpiar notificaciones asociadas)
        try
        {
            await _notificacionSincronizacionService.SincronizarEliminacionUsuarioAsync(usuarioId);
        }
        catch (Exception ex)
        {
            // Log del error pero no fallamos completamente
            // En producción, considerar usar un patrón de compensación más robusto
            Console.WriteLine($"Error al sincronizar eliminación de usuario {usuarioId} en MongoDB: {ex.Message}");
            throw;
        }
    }
}

