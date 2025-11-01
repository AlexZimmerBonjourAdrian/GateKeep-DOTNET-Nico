using GateKeep.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GateKeep.Api.Infrastructure.Persistence.Configurations;

public sealed class UsuarioEspacioConfiguration : IEntityTypeConfiguration<UsuarioEspacio>
{
    public void Configure(EntityTypeBuilder<UsuarioEspacio> builder)
    {
        builder.ToTable("usuarios_espacios");
        builder.HasKey(x => new { x.UsuarioId, x.EspacioId });

        // Índices individuales para optimizar consultas por usuario o espacio
        builder.HasIndex(x => x.UsuarioId)
            .HasDatabaseName("IX_usuarios_espacios_usuario_id");
            
        builder.HasIndex(x => x.EspacioId)
            .HasDatabaseName("IX_usuarios_espacios_espacio_id");

        // Relación Many-to-Many: Usuario-Espacio
        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Espacio>()
            .WithMany()
            .HasForeignKey(x => x.EspacioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}


