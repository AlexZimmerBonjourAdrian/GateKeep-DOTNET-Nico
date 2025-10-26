using GateKeep.Api.Domain.Entities;

namespace GateKeep.Api.Application.Espacios;

/// <summary>
/// Repository para el acceso a datos de espacios
/// </summary>
public interface IEspacioRepository
{
    /// <summary>
    /// Obtiene un edificio por su ID
    /// </summary>
    Task<Edificio?> ObtenerEdificioPorIdAsync(long id);

    /// <summary>
    /// Obtiene un edificio por su código
    /// </summary>
    Task<Edificio?> ObtenerEdificioPorCodigoAsync(string codigo);

    /// <summary>
    /// Obtiene un salón por su número en un edificio específico
    /// </summary>
    Task<Salon?> ObtenerSalonPorNumeroAsync(long edificioId, int numeroSalon);

    /// <summary>
    /// Obtiene un laboratorio por su número en un edificio específico
    /// </summary>
    Task<Laboratorio?> ObtenerLaboratorioPorNumeroAsync(long edificioId, int numeroLaboratorio);

    /// <summary>
    /// Obtiene todos los espacios
    /// </summary>
    Task<IEnumerable<Espacio>> ObtenerTodosAsync();

    /// <summary>
    /// Obtiene un espacio por su ID
    /// </summary>
    Task<Espacio?> ObtenerPorIdAsync(long id);

    /// <summary>
    /// Crea un nuevo espacio
    /// </summary>
    Task<Espacio> CrearAsync(Espacio espacio);

    /// <summary>
    /// Guarda un edificio y asigna el ID automáticamente
    /// </summary>
    Task<Edificio> GuardarEdificioAsync(Edificio edificio);

    /// <summary>
    /// Guarda un salón y asigna el ID automáticamente
    /// </summary>
    Task<Salon> GuardarSalonAsync(Salon salon);

    /// <summary>
    /// Guarda un laboratorio y asigna el ID automáticamente
    /// </summary>
    Task<Laboratorio> GuardarLaboratorioAsync(Laboratorio laboratorio);

    /// <summary>
    /// Actualiza un espacio existente
    /// </summary>
    Task<Espacio?> ActualizarAsync(Espacio espacio);

    /// <summary>
    /// Elimina un espacio
    /// </summary>
    Task<bool> EliminarAsync(long id);
}
