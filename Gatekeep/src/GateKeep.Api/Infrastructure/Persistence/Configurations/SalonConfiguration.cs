using GateKeep.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GateKeep.Api.Infrastructure.Persistence.Configurations;

public sealed class SalonConfiguration : IEntityTypeConfiguration<Salon>
{
    public void Configure(EntityTypeBuilder<Salon> builder)
    {
        builder.ToTable("salones");
        
        // TPT: No configurar clave aquí, se hereda de la tabla base
        
        // Propiedades específicas de Salon
        builder.Property(x => x.EdificioId)
            .IsRequired();
            
        builder.Property(x => x.NumeroSalon)
            .IsRequired();
            
        builder.Property(x => x.TipoSalon)
            .HasMaxLength(50);
            
        // Índices para optimizar consultas
        builder.HasIndex(x => new { x.EdificioId, x.NumeroSalon })
            .HasDatabaseName("IX_salones_edificio_numero")
            .IsUnique();
            
        builder.HasIndex(x => x.EdificioId)
            .HasDatabaseName("IX_salones_edificio_id");
            
        // Relación con Edificio
        builder.HasOne<Edificio>()
            .WithMany()
            .HasForeignKey(x => x.EdificioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
