using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GateKeep.Api.Domain.Entities;

public class Notificacion
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("mensaje")]
    public string Mensaje { get; set; } = string.Empty;

    [BsonElement("fechaEnvio")]
    public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;

    [BsonElement("tipo")]
    public string Tipo { get; set; } = "general";

    [BsonElement("activa")]
    public bool Activa { get; set; } = true;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}


