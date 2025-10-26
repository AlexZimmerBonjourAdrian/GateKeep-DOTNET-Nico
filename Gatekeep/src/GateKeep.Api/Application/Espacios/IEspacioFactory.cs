using GateKeep.Api.Contracts.Espacios;
using GateKeep.Api.Domain.Entities;

namespace GateKeep.Api.Application.Espacios;

/// <summary>
/// Factory para la creación de diferentes tipos de espacios (Edificio, Salón, Laboratorio)
/// </summary>
public interface IEspacioFactory
{
    /// <summary>
    /// Crea un nuevo edificio
    /// </summary>
    /// <param name="request">Datos para crear el edificio</param>
    /// <returns>Nueva instancia de Edificio</returns>
    Task<Edificio> CrearEdificioAsync(CrearEdificioRequest request);

    /// <summary>
    /// Crea un nuevo salón
    /// </summary>
    /// <param name="request">Datos para crear el salón</param>
    /// <returns>Nueva instancia de Salón</returns>
    Task<Salon> CrearSalonAsync(CrearSalonRequest request);

    /// <summary>
    /// Crea un nuevo laboratorio
    /// </summary>
    /// <param name="request">Datos para crear el laboratorio</param>
    /// <returns>Nueva instancia de Laboratorio</returns>
    Task<Laboratorio> CrearLaboratorioAsync(CrearLaboratorioRequest request);

    /// <summary>
    /// Intenta crear un espacio basado en el tipo especificado
    /// </summary>
    /// <param name="tipoEspacio">Tipo de espacio (edificio, salon, laboratorio)</param>
    /// <param name="request">Datos para crear el espacio</param>
    /// <param name="espacio">Espacio creado si es exitoso</param>
    /// <returns>True si la creación fue exitosa</returns>
    bool TryCrear(string tipoEspacio, CrearEspacioRequest request, out Task<Espacio>? espacio);

    /// <summary>
    /// Valida si un tipo de espacio es válido
    /// </summary>
    /// <param name="tipoEspacio">Tipo de espacio a validar</param>
    /// <returns>True si el tipo es válido</returns>
    bool EsTipoValido(string tipoEspacio);
}
