namespace GateKeep.Api.Domain.Entities;

public sealed record EventoAcceso
(
    long Id,
    string Nombre,
    DateTime Fecha,
    string Resultado,
    string PuntoControl,
    long UsuarioId,
    long EspacioId
);


