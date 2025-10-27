namespace GateKeep.Api.Contracts.Notificaciones;

public record NotificacionDto
{
    public string Id { get; init; } = string.Empty;
    public required string Mensaje { get; init; }
    public DateTime FechaEnvio { get; init; }
    public string Tipo { get; init; } = "general";
    public bool Activa { get; init; } = true;
}


