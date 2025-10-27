namespace GateKeep.Api.Contracts.Notificaciones;

public record NotificacionUsuarioDto
{
    public string Id { get; init; } = string.Empty;
    public long UsuarioId { get; init; }
    public string NotificacionId { get; init; } = string.Empty;
    public bool Leido { get; init; }
    public DateTime? FechaLectura { get; init; }
}


