using GateKeep.Api.Application.Anuncios;
using GateKeep.Api.Domain.Entities;
using GateKeep.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GateKeep.Api.Infrastructure.Anuncios;

public sealed class AnuncioRepository : IAnuncioRepository
{
    private readonly GateKeepDbContext _context;

    public AnuncioRepository(GateKeepDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Anuncio>> ObtenerTodosAsync()
    {
        return await _context.Anuncios
            .Where(a => a.Activo)
            .ToListAsync();
    }

    public async Task<Anuncio?> ObtenerPorIdAsync(long id)
    {
        return await _context.Anuncios.FindAsync(id);
    }

    public async Task<Anuncio> CrearAsync(Anuncio anuncio)
    {
        _context.Anuncios.Add(anuncio);
        await _context.SaveChangesAsync();
        return anuncio;
    }

    public async Task<Anuncio> ActualizarAsync(Anuncio anuncio)
    {
        var existing = await _context.Anuncios.FindAsync(anuncio.Id);
        if (existing is not null)
        {
            _context.Entry(existing).CurrentValues.SetValues(anuncio);
            await _context.SaveChangesAsync();
            return existing;
        }
        
        _context.Anuncios.Update(anuncio);
        await _context.SaveChangesAsync();
        return anuncio;
    }

    public async Task EliminarAsync(long id)
    {
        var anuncio = await _context.Anuncios.FindAsync(id);
        if (anuncio is not null)
        {
            // Soft delete: marcar como inactivo en lugar de eliminar
            _context.Entry(anuncio).Property(a => a.Activo).CurrentValue = false;
            await _context.SaveChangesAsync();
        }
    }
}

