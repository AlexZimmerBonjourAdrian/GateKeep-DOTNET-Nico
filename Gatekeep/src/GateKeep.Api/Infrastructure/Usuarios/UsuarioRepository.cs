using GateKeep.Api.Application.Usuarios;
using GateKeep.Api.Domain.Entities;
using GateKeep.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GateKeep.Api.Infrastructure.Usuarios;

public sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly GateKeepDbContext _context;

    public UsuarioRepository(GateKeepDbContext context)
    {
        _context = context;
    }

    public async Task<List<Usuario>> GetAllAsync()
    {
        return await _context.Usuarios.ToListAsync();
    }

    public async Task<Usuario?> GetByIdAsync(long id)
    {
        return await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        return await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task AddAsync(Usuario usuario)
    {
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Usuario usuario)
    {
        // Evitar conflicto de tracking: si ya hay una instancia con el mismo Id, actualizar sus valores
        var existing = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == usuario.Id);
        if (existing is not null)
        {
            _context.Entry(existing).CurrentValues.SetValues(usuario);
        }
        else
        {
            // No hay instancia trackeada; adjuntar y marcar como modificada
            _context.Usuarios.Update(usuario);
        }
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
        if (usuario is not null)
        {
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
        }
    }
}
