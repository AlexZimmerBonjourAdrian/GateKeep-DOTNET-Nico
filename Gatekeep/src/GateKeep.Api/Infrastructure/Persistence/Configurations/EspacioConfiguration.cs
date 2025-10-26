using GateKeep.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GateKeep.Api.Infrastructure.Persistence.Configurations;

public sealed class EspacioConfiguration : IEntityTypeConfiguration<Espacio>
{
    public void Configure(EntityTypeBuilder<Espacio> builder)
    {
        // TPT: Solo mapear la tabla base con propiedades comunes
        builder.ToTable("espacios");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        // Propiedades comunes de la clase base
        builder.Property(x => x.Nombre)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(x => x.Descripcion)
            .HasMaxLength(500);
            
        builder.Property(x => x.Ubicacion)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(x => x.Capacidad)
            .IsRequired();
            
        builder.Property(x => x.Activo)
            .IsRequired()
            .HasDefaultValue(true);

        // Índices para optimizar consultas comunes
        builder.HasIndex(x => x.Activo)
            .HasDatabaseName("IX_espacios_activo");
            
        builder.HasIndex(x => x.Nombre)
            .HasDatabaseName("IX_espacios_nombre");
    }
}


