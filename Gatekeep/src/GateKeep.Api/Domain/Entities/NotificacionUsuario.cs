using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GateKeep.Api.Domain.Entities;

public class NotificacionUsuario
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("usuarioId")]
    public long UsuarioId { get; set; }

    [BsonElement("notificacionId")]
    public string NotificacionId { get; set; } = string.Empty;

    [BsonElement("leido")]
    public bool Leido { get; set; } = false;

    [BsonElement("fechaLectura")]
    public DateTime? FechaLectura { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}


