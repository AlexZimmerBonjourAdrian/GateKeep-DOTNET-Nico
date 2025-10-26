namespace GateKeep.Api.Domain.Entities;

/// <summary>
/// Entidad que representa un salón
/// </summary>
public sealed record Salon(
    long Id,
    string Nombre,
    string? Descripcion,
    string Ubicacion,
    int Capacidad,
    long EdificioId,
    int NumeroSalon,
    string? TipoSalon,
    bool Activo = true
) : Espacio(Id, Nombre, Descripcion, Ubicacion, Capacidad, Activo);


