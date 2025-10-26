namespace GateKeep.Api.Domain.Entities;

/// <summary>
/// Entidad que representa un edificio
/// </summary>
public sealed record Edificio(
    long Id,
    string Nombre,
    string? Descripcion,
    string Ubicacion,
    int Capacidad,
    int NumeroPisos,
    string? CodigoEdificio,
    bool Activo = true
) : Espacio(Id, Nombre, Descripcion, Ubicacion, Capacidad, Activo);


