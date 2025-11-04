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

    public async Task<ReglaAcceso?> ObtenerPorEspacioIdAsync(long espacioId)
    {
        return await _context.ReglasAcceso
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
        _context.ReglasAcceso.Update(reglaAcceso);
        await _context.SaveChangesAsync();
        return reglaAcceso;
    }

    public async Task<bool> EliminarAsync(long id)
    {
        var regla = await _context.ReglasAcceso.FindAsync(id);
        if (regla == null)
            return false;

        _context.ReglasAcceso.Remove(regla);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ReglaAcceso>> ObtenerTodasAsync()
    {
        return await _context.ReglasAcceso.ToListAsync();
    }
}

