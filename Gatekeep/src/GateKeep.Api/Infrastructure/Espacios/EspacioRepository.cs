using GateKeep.Api.Application.Espacios;
using GateKeep.Api.Domain.Entities;
using GateKeep.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GateKeep.Api.Infrastructure.Espacios;

/// <summary>
/// Implementación del repositorio de espacios usando Entity Framework
/// </summary>
public sealed class EspacioRepository : IEspacioRepository
{
    private readonly GateKeepDbContext _context;

    public EspacioRepository(GateKeepDbContext context)
    {
        _context = context;
    }

    public async Task<Edificio?> ObtenerEdificioPorIdAsync(long id)
    {
        return await _context.Edificios.FindAsync(id);
    }

    public async Task<Edificio?> ObtenerEdificioPorCodigoAsync(string codigo)
    {
        return await _context.Edificios
            .FirstOrDefaultAsync(e => e.CodigoEdificio == codigo);
    }

    public async Task<Salon?> ObtenerSalonPorNumeroAsync(long edificioId, int numeroSalon)
    {
        return await _context.Salones
            .FirstOrDefaultAsync(s => s.EdificioId == edificioId && s.NumeroSalon == numeroSalon);
    }

    public async Task<Laboratorio?> ObtenerLaboratorioPorNumeroAsync(long edificioId, int numeroLaboratorio)
    {
        return await _context.Laboratorios
            .FirstOrDefaultAsync(l => l.EdificioId == edificioId && l.NumeroLaboratorio == numeroLaboratorio);
    }

    public async Task<IEnumerable<Espacio>> ObtenerTodosAsync()
    {
        // TPT: Una sola consulta optimizada usando Set<Espacio>()
        return await _context.Set<Espacio>()
            .Where(e => e.Activo)
            .ToListAsync();
    }

    public async Task<Espacio?> ObtenerPorIdAsync(long id)
    {
        // TPT: Una sola consulta optimizada usando Set<Espacio>()
        return await _context.Set<Espacio>()
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Espacio> CrearAsync(Espacio espacio)
    {
        switch (espacio)
        {
            case Edificio edificio:
                _context.Edificios.Add(edificio);
                break;
            case Salon salon:
                _context.Salones.Add(salon);
                break;
            case Laboratorio laboratorio:
                _context.Laboratorios.Add(laboratorio);
                break;
            default:
                throw new ArgumentException($"Tipo de espacio no soportado: {espacio.GetType().Name}");
        }

        await _context.SaveChangesAsync();
        return espacio;
    }

    public async Task<Espacio?> ActualizarAsync(Espacio espacio)
    {
        switch (espacio)
        {
            case Edificio edificio:
                _context.Edificios.Update(edificio);
                break;
            case Salon salon:
                _context.Salones.Update(salon);
                break;
            case Laboratorio laboratorio:
                _context.Laboratorios.Update(laboratorio);
                break;
            default:
                throw new ArgumentException($"Tipo de espacio no soportado: {espacio.GetType().Name}");
        }

        await _context.SaveChangesAsync();
        return espacio;
    }

    public async Task<Edificio> GuardarEdificioAsync(Edificio edificio)
    {
        _context.Edificios.Add(edificio);
        await _context.SaveChangesAsync();
        return edificio; // Ahora tiene el ID asignado por EF
    }

    public async Task<Salon> GuardarSalonAsync(Salon salon)
    {
        _context.Salones.Add(salon);
        await _context.SaveChangesAsync();
        return salon; // Ahora tiene el ID asignado por EF
    }

    public async Task<Laboratorio> GuardarLaboratorioAsync(Laboratorio laboratorio)
    {
        _context.Laboratorios.Add(laboratorio);
        await _context.SaveChangesAsync();
        return laboratorio; // Ahora tiene el ID asignado por EF
    }

    public async Task<bool> EliminarAsync(long id)
    {
        // TPT: Buscar y eliminar usando Set<Espacio>()
        var espacio = await _context.Set<Espacio>().FindAsync(id);
        if (espacio is not null)
        {
            _context.Set<Espacio>().Remove(espacio);
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    #region Métodos específicos optimizados para TPT

    /// <summary>
    /// Obtiene todos los edificios activos
    /// </summary>
    public async Task<IEnumerable<Edificio>> ObtenerEdificiosAsync()
    {
        return await _context.Set<Espacio>()
            .OfType<Edificio>()
            .Where(e => e.Activo)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene todos los salones activos
    /// </summary>
    public async Task<IEnumerable<Salon>> ObtenerSalonesAsync()
    {
        return await _context.Set<Espacio>()
            .OfType<Salon>()
            .Where(s => s.Activo)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene todos los laboratorios activos
    /// </summary>
    public async Task<IEnumerable<Laboratorio>> ObtenerLaboratoriosAsync()
    {
        return await _context.Set<Espacio>()
            .OfType<Laboratorio>()
            .Where(l => l.Activo)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene salones por edificio
    /// </summary>
    public async Task<IEnumerable<Salon>> ObtenerSalonesPorEdificioAsync(long edificioId)
    {
        return await _context.Set<Espacio>()
            .OfType<Salon>()
            .Where(s => s.EdificioId == edificioId && s.Activo)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene laboratorios por edificio
    /// </summary>
    public async Task<IEnumerable<Laboratorio>> ObtenerLaboratoriosPorEdificioAsync(long edificioId)
    {
        return await _context.Set<Espacio>()
            .OfType<Laboratorio>()
            .Where(l => l.EdificioId == edificioId && l.Activo)
            .ToListAsync();
    }

    #endregion
}
