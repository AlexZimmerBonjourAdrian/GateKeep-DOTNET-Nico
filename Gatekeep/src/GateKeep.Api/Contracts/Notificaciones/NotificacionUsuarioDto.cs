namespace GateKeep.Api.Contracts.Notificaciones;

public record NotificacionUsuarioDto
{
    public long UsuarioId { get; init; }
    public long NotificacionId { get; init; }
    public bool Leido { get; init; }
}


