namespace GateKeep.Api.Domain.Entities;

/// <summary>
/// Entidad que representa un evento general/deportivo
/// </summary>
public sealed record Evento
(
    long Id,
    string Nombre,
    DateTime Fecha,
    string Resultado,
    string PuntoControl,
    bool Activo = true
);

