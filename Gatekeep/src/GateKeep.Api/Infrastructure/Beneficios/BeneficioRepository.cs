using GateKeep.Api.Application.Beneficios;
using GateKeep.Api.Domain.Entities;
using GateKeep.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GateKeep.Api.Infrastructure.Beneficios;

public sealed class BeneficioRepository : IBeneficioRepository
{
    private readonly GateKeepDbContext _context;

    public BeneficioRepository(GateKeepDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Beneficio>> ObtenerTodosAsync()
    {
        return await _context.Beneficios.ToListAsync();
    }

    public async Task<Beneficio?> ObtenerPorIdAsync(long id)
    {
        return await _context.Beneficios.FindAsync(id);
    }

    public async Task<Beneficio> CrearAsync(Beneficio beneficio)
    {
        _context.Beneficios.Add(beneficio);
        await _context.SaveChangesAsync();
        return beneficio;
    }

    public async Task<Beneficio> ActualizarAsync(Beneficio beneficio)
    {
        var existing = await _context.Beneficios.FindAsync(beneficio.Id);
        if (existing is not null)
        {
            _context.Entry(existing).CurrentValues.SetValues(beneficio);
            await _context.SaveChangesAsync();
            return existing;
        }
        
        _context.Beneficios.Update(beneficio);
        await _context.SaveChangesAsync();
        return beneficio;
    }

    public async Task EliminarAsync(long id)
    {
        var beneficio = await _context.Beneficios.FindAsync(id);
        if (beneficio is not null)
        {
            _context.Beneficios.Remove(beneficio);
            await _context.SaveChangesAsync();
        }
    }
}
