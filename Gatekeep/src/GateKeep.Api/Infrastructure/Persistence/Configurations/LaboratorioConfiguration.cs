using GateKeep.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GateKeep.Api.Infrastructure.Persistence.Configurations;

public sealed class LaboratorioConfiguration : IEntityTypeConfiguration<Laboratorio>
{
    public void Configure(EntityTypeBuilder<Laboratorio> builder)
    {
        builder.ToTable("laboratorios");
        
        // TPT: No configurar clave aquí, se hereda de la tabla base
        
        // Propiedades específicas de Laboratorio
        builder.Property(x => x.EdificioId)
            .IsRequired();
            
        builder.Property(x => x.NumeroLaboratorio)
            .IsRequired();
            
        builder.Property(x => x.TipoLaboratorio)
            .HasMaxLength(50);
            
        builder.Property(x => x.EquipamientoEspecial)
            .IsRequired()
            .HasDefaultValue(false);
            
        // Índices para optimizar consultas
        builder.HasIndex(x => new { x.EdificioId, x.NumeroLaboratorio })
            .HasDatabaseName("IX_laboratorios_edificio_numero")
            .IsUnique();
            
        builder.HasIndex(x => x.EdificioId)
            .HasDatabaseName("IX_laboratorios_edificio_id");
            
        // Relación con Edificio
        builder.HasOne<Edificio>()
            .WithMany()
            .HasForeignKey(x => x.EdificioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
