namespace GateKeep.Api.Contracts.Notificaciones;

public record CrearNotificacionRequest
{
    public required string Mensaje { get; init; }
    public string Tipo { get; init; } = "general";
}
