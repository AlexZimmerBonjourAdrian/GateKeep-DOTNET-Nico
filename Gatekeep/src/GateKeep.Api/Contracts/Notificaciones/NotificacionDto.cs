namespace GateKeep.Api.Contracts.Notificaciones;

public sealed record NotificacionDto
{
    public long Id { get; init; }
    public required string Mensaje { get; init; }
    public DateTime FechaEnvio { get; init; }
}


