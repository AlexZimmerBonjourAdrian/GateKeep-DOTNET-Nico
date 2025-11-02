using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GateKeep.Api.Domain.Entities;

public class EventoHistorico
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("tipoEvento")]
    [BsonRequired]
    public string TipoEvento { get; set; } = string.Empty;

    [BsonElement("fecha")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    [BsonRequired]
    public DateTime Fecha { get; set; }

    [BsonElement("usuarioId")]
    [BsonRequired]
    public long UsuarioId { get; set; }

    [BsonElement("espacioId")]
    public long? EspacioId { get; set; }

    [BsonElement("resultado")]
    [BsonRequired]
    public string Resultado { get; set; } = string.Empty;

    [BsonElement("puntoControl")]
    public string? PuntoControl { get; set; }

    [BsonElement("datos")]
    public BsonDocument? Datos { get; set; }

    [BsonElement("createdAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("expireAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? ExpireAt { get; set; }
}

