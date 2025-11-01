using GateKeep.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GateKeep.Api.Infrastructure.Persistence.Configurations;

public sealed class EdificioConfiguration : IEntityTypeConfiguration<Edificio>
{
    public void Configure(EntityTypeBuilder<Edificio> builder)
    {
        builder.ToTable("edificios");
        builder.HasBaseType<Espacio>();
        
        // TPT: La clave se hereda de la tabla base Espacio
        
        // Propiedades específicas de Edificio
        builder.Property(x => x.NumeroPisos)
            .IsRequired();
            
        builder.Property(x => x.CodigoEdificio)
            .HasMaxLength(50);
            
        // Índices para optimizar consultas
        builder.HasIndex(x => x.CodigoEdificio)
            .HasDatabaseName("IX_edificios_codigo")
            .IsUnique()
            .HasFilter("\"CodigoEdificio\" IS NOT NULL");
    }
}
