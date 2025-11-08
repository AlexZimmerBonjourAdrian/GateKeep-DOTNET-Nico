namespace GateKeep.Api.Domain.Entities;

/// <summary>
/// Entidad que representa un anuncio
/// </summary>
public sealed record Anuncio
(
    long Id,
    string Nombre,
    DateTime Fecha,
    bool Activo = true
);

