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
        return await _context.Eventos.ToListAsync();
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
        _context.Eventos.Update(evento);
        await _context.SaveChangesAsync();
        return evento;
    }

    public async Task EliminarAsync(long id)
    {
        var evento = await _context.Eventos.FindAsync(id);
        if (evento is not null)
        {
            _context.Eventos.Remove(evento);
            await _context.SaveChangesAsync();
        }
    }
}

