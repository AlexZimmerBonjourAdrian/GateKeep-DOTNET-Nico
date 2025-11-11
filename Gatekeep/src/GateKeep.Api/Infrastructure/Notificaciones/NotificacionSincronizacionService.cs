using GateKeep.Api.Application.Notificaciones;
using GateKeep.Api.Application.Usuarios;
using GateKeep.Api.Application.Queues;
using GateKeep.Api.Domain.Entities;
using GateKeep.Api.Infrastructure.Observability;
using MongoDB.Driver;

namespace GateKeep.Api.Infrastructure.Notificaciones;

public sealed class NotificacionSincronizacionService : INotificacionSincronizacionService
{
    private readonly INotificacionUsuarioRepository _notificacionUsuarioRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IMongoCollection<NotificacionUsuario> _collection;
    private readonly IObservabilityService _observabilityService;
    private readonly ISincronizacionQueue _sincronizacionQueue;

    public NotificacionSincronizacionService(
        INotificacionUsuarioRepository notificacionUsuarioRepository,
        IUsuarioRepository usuarioRepository,
        IMongoDatabase mongoDatabase,
        IObservabilityService observabilityService,
        ISincronizacionQueue sincronizacionQueue)
    {
        _notificacionUsuarioRepository = notificacionUsuarioRepository;
        _usuarioRepository = usuarioRepository;
        _collection = mongoDatabase.GetCollection<NotificacionUsuario>("notificaciones_usuarios");
        _observabilityService = observabilityService;
        _sincronizacionQueue = sincronizacionQueue;
    }

    public async Task LimpiarRegistrosHuerfanosAsync(long usuarioId)
    {
        // Encolar en lugar de ejecutar directamente
        _sincronizacionQueue.Enqueue("limpiar_huerfanos", usuarioId);
    }

    public async Task ValidarConsistenciaAsync(long usuarioId)
    {
        // Encolar en lugar de ejecutar directamente
        _sincronizacionQueue.Enqueue("notificaciones", usuarioId);
    }

    public async Task SincronizarEliminacionUsuarioAsync(long usuarioId)
    {
        // Encolar en lugar de ejecutar directamente
        _sincronizacionQueue.Enqueue("eliminacion_usuario", usuarioId);
    }

    // Métodos internos para procesamiento desde la cola (llamados por el procesador)
    internal async Task ProcesarLimpiarRegistrosHuerfanosAsync(long usuarioId)
    {
        _observabilityService.RecordSincronizacionIniciada("notificaciones");
        try
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
        finally
        {
            _observabilityService.RecordSincronizacionCompletada("notificaciones");
        }
    }

    internal async Task ProcesarValidarConsistenciaAsync(long usuarioId)
    {
        _observabilityService.RecordSincronizacionIniciada("notificaciones");
        try
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
        finally
        {
            _observabilityService.RecordSincronizacionCompletada("notificaciones");
        }
    }

    internal async Task ProcesarSincronizarEliminacionUsuarioAsync(long usuarioId)
    {
        _observabilityService.RecordSincronizacionIniciada("notificaciones");
        try
        {
            // Verificar que el usuario realmente fue eliminado
            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
            
            if (usuario is null)
            {
                // El usuario fue eliminado, limpiar sus notificaciones en MongoDB
                await ProcesarLimpiarRegistrosHuerfanosAsync(usuarioId);
            }
            else
            {
                throw new InvalidOperationException($"No se puede sincronizar eliminación: El usuario {usuarioId} todavía existe en PostgreSQL");
            }
        }
        finally
        {
            _observabilityService.RecordSincronizacionCompletada("notificaciones");
        }
    }
}

