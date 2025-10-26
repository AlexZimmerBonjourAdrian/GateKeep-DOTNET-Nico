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
        var edificios = await _context.Edificios.ToListAsync();
        var salones = await _context.Salones.ToListAsync();
        var laboratorios = await _context.Laboratorios.ToListAsync();

        return edificios.Cast<Espacio>()
            .Concat(salones.Cast<Espacio>())
            .Concat(laboratorios.Cast<Espacio>());
    }

    public async Task<Espacio?> ObtenerPorIdAsync(long id)
    {
        // Buscar en todas las tablas de espacios
        var edificio = await _context.Edificios.FindAsync(id);
        if (edificio is not null) return edificio;

        var salon = await _context.Salones.FindAsync(id);
        if (salon is not null) return salon;

        var laboratorio = await _context.Laboratorios.FindAsync(id);
        return laboratorio;
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
        // Buscar y eliminar en todas las tablas
        var edificio = await _context.Edificios.FindAsync(id);
        if (edificio is not null)
        {
            _context.Edificios.Remove(edificio);
            await _context.SaveChangesAsync();
            return true;
        }

        var salon = await _context.Salones.FindAsync(id);
        if (salon is not null)
        {
            _context.Salones.Remove(salon);
            await _context.SaveChangesAsync();
            return true;
        }

        var laboratorio = await _context.Laboratorios.FindAsync(id);
        if (laboratorio is not null)
        {
            _context.Laboratorios.Remove(laboratorio);
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }
}
