using GateKeep.Api.Application.Eventos;
using GateKeep.Api.Domain.Entities;
using GateKeep.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GateKeep.Api.Infrastructure.Eventos;

public sealed class EventoRepository : IEventoRepository
{
    private readonly GateKeepDbContext _context;

    public EventoRepository(GateKeepDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Evento>> ObtenerTodosAsync()
    {
        return await _context.Eventos
            .Where(e => e.Activo)
            .ToListAsync();
    }

    public async Task<Evento?> ObtenerPorIdAsync(long id)
    {
        return await _context.Eventos.FindAsync(id);
    }

    public async Task<Evento> CrearAsync(Evento evento)
    {
        _context.Eventos.Add(evento);
        await _context.SaveChangesAsync();
        return evento;
    }

    public async Task<Evento> ActualizarAsync(Evento evento)
    {
        var existing = await _context.Eventos.FindAsync(evento.Id);
        if (existing is not null)
        {
            _context.Entry(existing).CurrentValues.SetValues(evento);
            await _context.SaveChangesAsync();
            return existing;
        }
        
        _context.Eventos.Update(evento);
        await _context.SaveChangesAsync();
        return evento;
    }

    public async Task EliminarAsync(long id)
    {
        var evento = await _context.Eventos.FindAsync(id);
        if (evento is not null)
        {
            // Soft delete: marcar como inactivo en lugar de eliminar
            _context.Entry(evento).Property(e => e.Activo).CurrentValue = false;
            await _context.SaveChangesAsync();
        }
    }
}

