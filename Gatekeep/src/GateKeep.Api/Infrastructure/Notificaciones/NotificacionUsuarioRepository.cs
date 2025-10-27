using GateKeep.Api.Application.Notificaciones;
using GateKeep.Api.Domain.Entities;
using MongoDB.Driver;

namespace GateKeep.Api.Infrastructure.Notificaciones;

public class NotificacionUsuarioRepository : INotificacionUsuarioRepository
{
    private readonly IMongoCollection<NotificacionUsuario> _collection;

    public NotificacionUsuarioRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<NotificacionUsuario>("notificaciones_usuarios");
    }

    public async Task<IEnumerable<NotificacionUsuario>> ObtenerPorUsuarioAsync(long usuarioId)
    {
        return await _collection.Find(nu => nu.UsuarioId == usuarioId).ToListAsync();
    }

    public async Task<NotificacionUsuario?> ObtenerPorUsuarioYNotificacionAsync(long usuarioId, string notificacionId)
    {
        return await _collection.Find(nu => nu.UsuarioId == usuarioId && nu.NotificacionId == notificacionId).FirstOrDefaultAsync();
    }

    public async Task<NotificacionUsuario> CrearAsync(NotificacionUsuario notificacionUsuario)
    {
        notificacionUsuario.CreatedAt = DateTime.UtcNow;
        notificacionUsuario.UpdatedAt = DateTime.UtcNow;
        await _collection.InsertOneAsync(notificacionUsuario);
        return notificacionUsuario;
    }

    public async Task<NotificacionUsuario> ActualizarAsync(NotificacionUsuario notificacionUsuario)
    {
        notificacionUsuario.UpdatedAt = DateTime.UtcNow;
        await _collection.ReplaceOneAsync(nu => nu.Id == notificacionUsuario.Id, notificacionUsuario);
        return notificacionUsuario;
    }

    public async Task<bool> MarcarComoLeidaAsync(long usuarioId, string notificacionId)
    {
        var filter = Builders<NotificacionUsuario>.Filter.And(
            Builders<NotificacionUsuario>.Filter.Eq(nu => nu.UsuarioId, usuarioId),
            Builders<NotificacionUsuario>.Filter.Eq(nu => nu.NotificacionId, notificacionId)
        );

        var update = Builders<NotificacionUsuario>.Update
            .Set(nu => nu.Leido, true)
            .Set(nu => nu.FechaLectura, DateTime.UtcNow)
            .Set(nu => nu.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }

    public async Task<IEnumerable<NotificacionUsuario>> ObtenerNoLeidasAsync(long usuarioId)
    {
        return await _collection.Find(nu => nu.UsuarioId == usuarioId && !nu.Leido).ToListAsync();
    }

    public async Task<int> ContarNoLeidasAsync(long usuarioId)
    {
        return (int)await _collection.CountDocumentsAsync(nu => nu.UsuarioId == usuarioId && !nu.Leido);
    }
}
