namespace GateKeep.Api.Domain.Entities;

public sealed record NotificacionUsuario
(
    long UsuarioId,
    long NotificacionId,
    bool Leido
);


