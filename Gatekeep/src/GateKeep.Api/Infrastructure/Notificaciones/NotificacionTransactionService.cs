using GateKeep.Api.Application.Notificaciones;
using GateKeep.Api.Domain.Entities;
using MongoDB.Driver;

namespace GateKeep.Api.Infrastructure.Notificaciones;

public sealed class NotificacionTransactionService : INotificacionTransactionService
{
    private readonly INotificacionUsuarioRepository _notificacionUsuarioRepository;
    private readonly INotificacionUsuarioValidationService _validationService;
    private readonly IMongoCollection<NotificacionUsuario> _collection;

    public NotificacionTransactionService(
        INotificacionUsuarioRepository notificacionUsuarioRepository,
        INotificacionUsuarioValidationService validationService,
        IMongoDatabase mongoDatabase)
    {
        _notificacionUsuarioRepository = notificacionUsuarioRepository;
        _validationService = validationService;
        _collection = mongoDatabase.GetCollection<NotificacionUsuario>("notificaciones_usuarios");
    }

    public async Task<NotificacionUsuario> CrearNotificacionUsuarioConCompensacionAsync(
        long usuarioId, 
        string notificacionId)
    {
        // Validar integridad referencial antes de crear
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
            return notificacionUsuarioCreado;
        }
        catch (Exception ex)
        {
            // Log del error
            Console.WriteLine($"Error al crear NotificacionUsuario: {ex.Message}");
            
            // No hay compensación necesaria aquí porque la creación es idempotente
            // Si falla, simplemente no se crea nada
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

