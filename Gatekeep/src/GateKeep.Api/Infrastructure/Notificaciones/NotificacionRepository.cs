using GateKeep.Api.Application.Notificaciones;
using GateKeep.Api.Domain.Entities;
using MongoDB.Driver;

namespace GateKeep.Api.Infrastructure.Notificaciones;

public class NotificacionRepository : INotificacionRepository
{
    private readonly IMongoCollection<Notificacion> _collection;

    public NotificacionRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Notificacion>("notificaciones");
    }

    public async Task<IEnumerable<Notificacion>> ObtenerTodasAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<Notificacion?> ObtenerPorIdAsync(string id)
    {
        return await _collection.Find(n => n.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Notificacion> CrearAsync(Notificacion notificacion)
    {
        notificacion.CreatedAt = DateTime.UtcNow;
        notificacion.UpdatedAt = DateTime.UtcNow;
        await _collection.InsertOneAsync(notificacion);
        return notificacion;
    }

    public async Task<Notificacion> ActualizarAsync(Notificacion notificacion)
    {
        notificacion.UpdatedAt = DateTime.UtcNow;
        await _collection.ReplaceOneAsync(n => n.Id == notificacion.Id, notificacion);
        return notificacion;
    }

    public async Task<bool> EliminarAsync(string id)
    {
        var result = await _collection.DeleteOneAsync(n => n.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<IEnumerable<Notificacion>> ObtenerActivasAsync()
    {
        return await _collection.Find(n => n.Activa).ToListAsync();
    }

    public async Task<IEnumerable<Notificacion>> ObtenerPorTipoAsync(string tipo)
    {
        return await _collection.Find(n => n.Tipo == tipo).ToListAsync();
    }
}
