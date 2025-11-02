using GateKeep.Api.Application.Auditoria;
using GateKeep.Api.Application.Notificaciones;
using GateKeep.Api.Domain.Entities;
using MongoDB.Driver;

namespace GateKeep.Api.Infrastructure.Notificaciones;

public sealed class NotificacionTransactionService : INotificacionTransactionService
{
    private readonly INotificacionUsuarioRepository _notificacionUsuarioRepository;
    private readonly INotificacionUsuarioValidationService _validationService;
    private readonly IMongoCollection<NotificacionUsuario> _collection;
    private readonly IEventoHistoricoService? _eventoHistoricoService;
    private readonly INotificacionRepository? _notificacionRepository;

    public NotificacionTransactionService(
        INotificacionUsuarioRepository notificacionUsuarioRepository,
        INotificacionUsuarioValidationService validationService,
        IMongoDatabase mongoDatabase,
        IEventoHistoricoService? eventoHistoricoService = null,
        INotificacionRepository? notificacionRepository = null)
    {
        _notificacionUsuarioRepository = notificacionUsuarioRepository;
        _validationService = validationService;
        _collection = mongoDatabase.GetCollection<NotificacionUsuario>("notificaciones_usuarios");
        _eventoHistoricoService = eventoHistoricoService;
        _notificacionRepository = notificacionRepository;
    }

    public async Task<NotificacionUsuario> CrearNotificacionUsuarioConCompensacionAsync(
        long usuarioId, 
        string notificacionId)
    {
        await _validationService.ValidarIntegridadReferencialAsync(usuarioId, notificacionId);

        try
        {
            var notificacionUsuario = new NotificacionUsuario
            {
                UsuarioId = usuarioId,
                NotificacionId = notificacionId,
                Leido = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var notificacionUsuarioCreado = await _notificacionUsuarioRepository.CrearAsync(notificacionUsuario);
            
            Console.WriteLine($"NotificacionUsuario creado exitosamente: UsuarioId={usuarioId}, NotificacionId={notificacionId}");
            
            if (_eventoHistoricoService != null && _notificacionRepository != null)
            {
                try
                {
                    var notificacion = await _notificacionRepository.ObtenerPorIdAsync(notificacionId);
                    if (notificacion != null)
                    {
                        await _eventoHistoricoService.RegistrarNotificacionAsync(
                            usuarioId,
                            notificacion.Tipo,
                            "Enviada",
                            new Dictionary<string, object> { { "notificacionId", notificacionId } });
                    }
                }
                catch
                {
                }
            }
            
            return notificacionUsuarioCreado;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al crear NotificacionUsuario: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> EliminarNotificacionUsuarioConCompensacionAsync(
        long usuarioId, 
        string notificacionId)
    {
        try
        {
            // Buscar el registro antes de eliminarlo (para posible rollback)
            var registroExistente = await _notificacionUsuarioRepository
                .ObtenerPorUsuarioYNotificacionAsync(usuarioId, notificacionId);
            
            if (registroExistente is null)
            {
                return false;
            }

            // Eliminar el registro
            var filter = Builders<NotificacionUsuario>.Filter.And(
                Builders<NotificacionUsuario>.Filter.Eq(nu => nu.UsuarioId, usuarioId),
                Builders<NotificacionUsuario>.Filter.Eq(nu => nu.NotificacionId, notificacionId)
            );

            var result = await _collection.DeleteOneAsync(filter);
            
            if (result.DeletedCount > 0)
            {
                Console.WriteLine($"NotificacionUsuario eliminado exitosamente: UsuarioId={usuarioId}, NotificacionId={notificacionId}");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            // Log del error
            Console.WriteLine($"Error al eliminar NotificacionUsuario: {ex.Message}");
            
            // En un escenario real, aquí podrías intentar restaurar el registro
            // usando registroExistente, pero MongoDB no soporta transacciones distribuidas
            // con PostgreSQL, así que solo registramos el error
            throw;
        }
    }
}

