using GateKeep.Api.Application.Notificaciones;
using GateKeep.Api.Application.Usuarios;
using GateKeep.Api.Domain.Entities;
using MongoDB.Driver;

namespace GateKeep.Api.Infrastructure.Notificaciones;

public sealed class NotificacionSincronizacionService : INotificacionSincronizacionService
{
    private readonly INotificacionUsuarioRepository _notificacionUsuarioRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IMongoCollection<NotificacionUsuario> _collection;

    public NotificacionSincronizacionService(
        INotificacionUsuarioRepository notificacionUsuarioRepository,
        IUsuarioRepository usuarioRepository,
        IMongoDatabase mongoDatabase)
    {
        _notificacionUsuarioRepository = notificacionUsuarioRepository;
        _usuarioRepository = usuarioRepository;
        _collection = mongoDatabase.GetCollection<NotificacionUsuario>("notificaciones_usuarios");
    }

    public async Task LimpiarRegistrosHuerfanosAsync(long usuarioId)
    {
        // Verificar si el usuario existe en PostgreSQL
        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
        
        if (usuario is null)
        {
            // El usuario no existe en PostgreSQL, eliminar registros huérfanos en MongoDB
            var filter = Builders<NotificacionUsuario>.Filter.Eq(nu => nu.UsuarioId, usuarioId);
            var result = await _collection.DeleteManyAsync(filter);
            
            // Log o notificación de que se eliminaron registros huérfanos
            Console.WriteLine($"Se eliminaron {result.DeletedCount} registros huérfanos para el usuario {usuarioId}");
        }
    }

    public async Task ValidarConsistenciaAsync(long usuarioId)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
        var notificacionesUsuario = await _notificacionUsuarioRepository.ObtenerPorUsuarioAsync(usuarioId);

        if (usuario is null && notificacionesUsuario.Any())
        {
            // Inconsistencia: hay notificaciones en MongoDB pero el usuario no existe en PostgreSQL
            throw new InvalidOperationException(
                $"Inconsistencia detectada: El usuario {usuarioId} no existe en PostgreSQL pero tiene {notificacionesUsuario.Count()} notificaciones en MongoDB");
        }
    }

    public async Task SincronizarEliminacionUsuarioAsync(long usuarioId)
    {
        // Verificar que el usuario realmente fue eliminado
        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
        
        if (usuario is null)
        {
            // El usuario fue eliminado, limpiar sus notificaciones en MongoDB
            await LimpiarRegistrosHuerfanosAsync(usuarioId);
        }
        else
        {
            throw new InvalidOperationException($"No se puede sincronizar eliminación: El usuario {usuarioId} todavía existe en PostgreSQL");
        }
    }
}

