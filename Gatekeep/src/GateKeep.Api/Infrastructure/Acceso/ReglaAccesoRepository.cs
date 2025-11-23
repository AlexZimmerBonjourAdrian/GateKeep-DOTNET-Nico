using GateKeep.Api.Application.Acceso;
using GateKeep.Api.Domain.Entities;
using GateKeep.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GateKeep.Api.Infrastructure.Acceso;

public class ReglaAccesoRepository : IReglaAccesoRepository
{
    private readonly GateKeepDbContext _context;

    public ReglaAccesoRepository(GateKeepDbContext context)
    {
        _context = context;
    }

    public async Task<ReglaAcceso?> ObtenerPorIdAsync(long id)
    {
        return await _context.ReglasAcceso
            .Where(r => r.Activo)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<ReglaAcceso?> ObtenerPorEspacioIdAsync(long espacioId)
    {
        return await _context.ReglasAcceso
            .Where(r => r.Activo)
            .FirstOrDefaultAsync(r => r.EspacioId == espacioId);
    }

    public async Task<ReglaAcceso> CrearAsync(ReglaAcceso reglaAcceso)
    {
        _context.ReglasAcceso.Add(reglaAcceso);
        await _context.SaveChangesAsync();
        return reglaAcceso;
    }

    public async Task<ReglaAcceso> ActualizarAsync(ReglaAcceso reglaAcceso)
    {
        var existing = await _context.ReglasAcceso.FindAsync(reglaAcceso.Id);
        if (existing is not null)
        {
            _context.Entry(existing).CurrentValues.SetValues(reglaAcceso);
            await _context.SaveChangesAsync();
            return existing;
        }
        
        _context.ReglasAcceso.Update(reglaAcceso);
        await _context.SaveChangesAsync();
        return reglaAcceso;
    }

    public async Task<bool> EliminarAsync(long id)
    {
        var regla = await _context.ReglasAcceso.FindAsync(id);
        if (regla == null)
            return false;

        _context.Entry(regla).Property(r => r.Activo).CurrentValue = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ReglaAcceso>> ObtenerTodasAsync()
    {
        return await _context.ReglasAcceso
            .Where(r => r.Activo)
            .ToListAsync();
    }
}

