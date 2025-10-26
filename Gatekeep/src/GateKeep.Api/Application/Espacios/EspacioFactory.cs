using GateKeep.Api.Contracts.Espacios;
using GateKeep.Api.Domain.Entities;

namespace GateKeep.Api.Application.Espacios;

/// <summary>
/// Factory para la creación de diferentes tipos de espacios
/// </summary>
public sealed class EspacioFactory : IEspacioFactory
{
    private readonly IEspacioRepository _repository;

    public EspacioFactory(IEspacioRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Crea un nuevo edificio
    /// </summary>
    public async Task<Edificio> CrearEdificioAsync(CrearEdificioRequest request)
    {
        // Validaciones específicas para edificios
        await ValidarEdificioAsync(request);

        // Crear con ID = 0, EF asignará el ID real al guardar
        return new Edificio(
            Id: 0, // EF manejará la asignación del ID
            Nombre: request.Nombre,
            Descripcion: request.Descripcion,
            Ubicacion: request.Ubicacion,
            Capacidad: request.Capacidad,
            NumeroPisos: request.NumeroPisos,
            CodigoEdificio: request.CodigoEdificio,
            Activo: request.Activo
        );
    }

    /// <summary>
    /// Crea un nuevo salón
    /// </summary>
    public async Task<Salon> CrearSalonAsync(CrearSalonRequest request)
    {
        // Validaciones específicas para salones
        await ValidarSalonAsync(request);

        // Crear con ID = 0, EF asignará el ID real al guardar
        return new Salon(
            Id: 0, // EF manejará la asignación del ID
            Nombre: request.Nombre,
            Descripcion: request.Descripcion,
            Ubicacion: request.Ubicacion,
            Capacidad: request.Capacidad,
            EdificioId: request.EdificioId,
            NumeroSalon: request.NumeroSalon,
            TipoSalon: request.TipoSalon,
            Activo: request.Activo
        );
    }

    /// <summary>
    /// Crea un nuevo laboratorio
    /// </summary>
    public async Task<Laboratorio> CrearLaboratorioAsync(CrearLaboratorioRequest request)
    {
        // Validaciones específicas para laboratorios
        await ValidarLaboratorioAsync(request);

        // Crear con ID = 0, EF asignará el ID real al guardar
        return new Laboratorio(
            Id: 0, // EF manejará la asignación del ID
            Nombre: request.Nombre,
            Descripcion: request.Descripcion,
            Ubicacion: request.Ubicacion,
            Capacidad: request.Capacidad,
            EdificioId: request.EdificioId,
            NumeroLaboratorio: request.NumeroLaboratorio,
            TipoLaboratorio: request.TipoLaboratorio,
            EquipamientoEspecial: request.EquipamientoEspecial,
            Activo: request.Activo
        );
    }

    /// <summary>
    /// Intenta crear un espacio basado en el tipo especificado
    /// </summary>
    public bool TryCrear(string tipoEspacio, CrearEspacioRequest request, out Task<Espacio>? espacio)
    {
        espacio = null;

        if (!EsTipoValido(tipoEspacio))
            return false;

        try
        {
            espacio = tipoEspacio.ToLower() switch
            {
                "edificio" => CrearEdificioAsync((CrearEdificioRequest)request).ContinueWith(t => (Espacio)t.Result),
                "salon" => CrearSalonAsync((CrearSalonRequest)request).ContinueWith(t => (Espacio)t.Result),
                "laboratorio" => CrearLaboratorioAsync((CrearLaboratorioRequest)request).ContinueWith(t => (Espacio)t.Result),
                _ => throw new ArgumentException($"Tipo de espacio no válido: {tipoEspacio}")
            };

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Valida si un tipo de espacio es válido
    /// </summary>
    public bool EsTipoValido(string tipoEspacio)
    {
        return tipoEspacio.ToLower() switch
        {
            "edificio" or "salon" or "laboratorio" => true,
            _ => false
        };
    }

    #region Validaciones Privadas

    private async Task ValidarEdificioAsync(CrearEdificioRequest request)
    {
        // Validar que no exista otro edificio con el mismo código
        if (!string.IsNullOrWhiteSpace(request.CodigoEdificio))
        {
            var edificioExistente = await _repository.ObtenerEdificioPorCodigoAsync(request.CodigoEdificio);
            if (edificioExistente is not null)
            {
                throw new InvalidOperationException($"Ya existe un edificio con código {request.CodigoEdificio}");
            }
        }

        // Validar número de pisos
        if (request.NumeroPisos <= 0)
        {
            throw new ArgumentException("El número de pisos debe ser mayor a 0");
        }

        // Validar capacidad
        if (request.Capacidad <= 0)
        {
            throw new ArgumentException("La capacidad debe ser mayor a 0");
        }
    }

    private async Task ValidarSalonAsync(CrearSalonRequest request)
    {
        // Validar que el edificio padre exista
        var edificio = await _repository.ObtenerEdificioPorIdAsync(request.EdificioId);
        if (edificio is null)
        {
            throw new ArgumentException($"No existe un edificio con ID {request.EdificioId}");
        }

        // Validar que no exista otro salón con el mismo número en el mismo edificio
        var salonExistente = await _repository.ObtenerSalonPorNumeroAsync(request.EdificioId, request.NumeroSalon);
        if (salonExistente is not null)
        {
            throw new InvalidOperationException($"Ya existe un salón con número {request.NumeroSalon} en el edificio {request.EdificioId}");
        }

        // Validar capacidad
        if (request.Capacidad <= 0)
        {
            throw new ArgumentException("La capacidad debe ser mayor a 0");
        }
    }

    private async Task ValidarLaboratorioAsync(CrearLaboratorioRequest request)
    {
        // Validar que el edificio padre exista
        var edificio = await _repository.ObtenerEdificioPorIdAsync(request.EdificioId);
        if (edificio is null)
        {
            throw new ArgumentException($"No existe un edificio con ID {request.EdificioId}");
        }

        // Validar que no exista otro laboratorio con el mismo número en el mismo edificio
        var laboratorioExistente = await _repository.ObtenerLaboratorioPorNumeroAsync(request.EdificioId, request.NumeroLaboratorio);
        if (laboratorioExistente is not null)
        {
            throw new InvalidOperationException($"Ya existe un laboratorio con número {request.NumeroLaboratorio} en el edificio {request.EdificioId}");
        }

        // Validar capacidad
        if (request.Capacidad <= 0)
        {
            throw new ArgumentException("La capacidad debe ser mayor a 0");
        }
    }

    #endregion
}
