namespace GateKeep.Api.Contracts.Notificaciones;

public record NotificacionCompletaDto
{
    public long UsuarioId { get; init; }
    public string UsuarioNombre { get; init; } = string.Empty;
    public string UsuarioEmail { get; init; } = string.Empty;
    public string NotificacionId { get; init; } = string.Empty;
    public string Mensaje { get; init; } = string.Empty;
    public string Tipo { get; init; } = string.Empty;
    public DateTime FechaEnvio { get; init; }
    public bool Leido { get; init; }
    public DateTime? FechaLectura { get; init; }
    public DateTime CreatedAt { get; init; }
}

