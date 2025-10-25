namespace GateKeep.Api.Domain.Entities;

public sealed record Notificacion
(
    long Id,
    string Mensaje,
    DateTime FechaEnvio
);


