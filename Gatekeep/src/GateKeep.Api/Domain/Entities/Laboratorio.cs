namespace GateKeep.Api.Domain.Entities;

/// <summary>
/// Entidad que representa un laboratorio
/// </summary>
public sealed record Laboratorio(
    long Id,
    string Nombre,
    string? Descripcion,
    string Ubicacion,
    int Capacidad,
    long EdificioId,
    int NumeroLaboratorio,
    string? TipoLaboratorio,
    bool EquipamientoEspecial,
    bool Activo = true
) : Espacio(Id, Nombre, Descripcion, Ubicacion, Capacidad, Activo);


