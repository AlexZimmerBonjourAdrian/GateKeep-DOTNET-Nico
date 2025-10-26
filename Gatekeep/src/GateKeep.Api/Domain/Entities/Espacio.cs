namespace GateKeep.Api.Domain.Entities;

/// <summary>
/// Clase base abstracta para todos los tipos de espacios
/// </summary>
public abstract record Espacio(
    long Id,
    string Nombre,
    string? Descripcion,
    string Ubicacion,
    int Capacidad,
    bool Activo = true
);


