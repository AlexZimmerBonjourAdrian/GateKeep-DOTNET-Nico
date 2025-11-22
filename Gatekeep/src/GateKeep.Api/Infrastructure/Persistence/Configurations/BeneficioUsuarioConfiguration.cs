using GateKeep.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GateKeep.Api.Infrastructure.Persistence.Configurations;

public sealed class BeneficioUsuarioConfiguration : IEntityTypeConfiguration<BeneficioUsuario>
{
    public void Configure(EntityTypeBuilder<BeneficioUsuario> builder)
    {
        builder.ToTable("beneficios_usuarios");
        builder.HasKey(x => new { x.UsuarioId, x.BeneficioId });
        builder.Property(x => x.EstadoCanje).IsRequired();

        // Índices individuales para optimizar consultas por usuario o beneficio
        builder.HasIndex(x => x.UsuarioId)
            .HasDatabaseName("IX_beneficios_usuarios_usuario_id");
            
        builder.HasIndex(x => x.BeneficioId)
            .HasDatabaseName("IX_beneficios_usuarios_beneficio_id");

        // Relación Many-to-Many: Beneficio-Usuario
        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Beneficio>()
            .WithMany()
            .HasForeignKey(x => x.BeneficioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}


