using GateKeep.Api.Application.Beneficios;
using GateKeep.Api.Domain.Entities;
using GateKeep.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GateKeep.Api.Infrastructure.Beneficios;

public sealed class BeneficioUsuarioRepository : IBeneficioUsuarioRepository
{
    private readonly GateKeepDbContext _context;

    public BeneficioUsuarioRepository(GateKeepDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<(BeneficioUsuario BeneficioUsuario, Beneficio? Beneficio)>> ObtenerPorUsuarioConDetallesAsync(long usuarioId)
    {
        var result = await _context.BeneficiosUsuarios
            .Where(bu => bu.UsuarioId == usuarioId)
            .Join(
                _context.Beneficios,
                bu => bu.BeneficioId,
                b => b.Id,
                (bu, b) => new { BeneficioUsuario = bu, Beneficio = (Beneficio?)b }
            )
            .ToListAsync();

        return result.Select(x => (x.BeneficioUsuario, x.Beneficio));
    }

    public async Task<IEnumerable<BeneficioUsuario>> ObtenerPorUsuarioAsync(long usuarioId)
    {
        return await _context.BeneficiosUsuarios
            .Where(bu => bu.UsuarioId == usuarioId)
            .ToListAsync();
    }

    public async Task<IEnumerable<BeneficioUsuario>> ObtenerPorBeneficioAsync(long beneficioId)
    {
        return await _context.BeneficiosUsuarios
            .Where(bu => bu.BeneficioId == beneficioId)
            .ToListAsync();
    }

    public async Task<BeneficioUsuario?> ObtenerAsync(long usuarioId, long beneficioId)
    {
        return await _context.BeneficiosUsuarios
            .FirstOrDefaultAsync(bu => bu.UsuarioId == usuarioId && bu.BeneficioId == beneficioId);
    }

    public async Task<BeneficioUsuario> CrearAsync(BeneficioUsuario beneficioUsuario)
    {
        _context.BeneficiosUsuarios.Add(beneficioUsuario);
        await _context.SaveChangesAsync();
        return beneficioUsuario;
    }

    public async Task<BeneficioUsuario> ActualizarAsync(BeneficioUsuario beneficioUsuario)
    {
        // Find the existing tracked entity if any
        var existingEntry = _context.ChangeTracker.Entries<BeneficioUsuario>()
            .FirstOrDefault(e => e.Entity.UsuarioId == beneficioUsuario.UsuarioId && 
                                 e.Entity.BeneficioId == beneficioUsuario.BeneficioId);
        
        if (existingEntry != null)
        {
            // Update the existing tracked entity
            existingEntry.CurrentValues.SetValues(beneficioUsuario);
        }
        else
        {
            // No existing tracked entity, attach and mark as modified
            _context.BeneficiosUsuarios.Update(beneficioUsuario);
        }
        
        await _context.SaveChangesAsync();
        return beneficioUsuario;
    }

    public async Task EliminarAsync(long usuarioId, long beneficioId)
    {
        var beneficioUsuario = await _context.BeneficiosUsuarios
            .FirstOrDefaultAsync(bu => bu.UsuarioId == usuarioId && bu.BeneficioId == beneficioId);
        
        if (beneficioUsuario is not null)
        {
            _context.BeneficiosUsuarios.Remove(beneficioUsuario);
            await _context.SaveChangesAsync();
        }
    }
}
