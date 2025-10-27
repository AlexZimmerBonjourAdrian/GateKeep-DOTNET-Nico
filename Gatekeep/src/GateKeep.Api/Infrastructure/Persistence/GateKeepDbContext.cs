using GateKeep.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GateKeep.Api.Infrastructure.Persistence;

public sealed class GateKeepDbContext : DbContext
{
    public GateKeepDbContext(DbContextOptions<GateKeepDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Beneficio> Beneficios => Set<Beneficio>();
    public DbSet<BeneficioUsuario> BeneficiosUsuarios => Set<BeneficioUsuario>();
    public DbSet<Espacio> Espacios => Set<Espacio>();
    public DbSet<Edificio> Edificios => Set<Edificio>();
    public DbSet<Laboratorio> Laboratorios => Set<Laboratorio>();
    public DbSet<Salon> Salones => Set<Salon>();
    public DbSet<ReglaAcceso> ReglasAcceso => Set<ReglaAcceso>();
    public DbSet<EventoAcceso> EventosAcceso => Set<EventoAcceso>();
    public DbSet<UsuarioEspacio> UsuariosEspacios => Set<UsuarioEspacio>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GateKeepDbContext).Assembly);
    }
}


