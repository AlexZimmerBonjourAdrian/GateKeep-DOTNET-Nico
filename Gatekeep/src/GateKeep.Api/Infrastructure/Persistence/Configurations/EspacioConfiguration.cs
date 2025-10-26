using GateKeep.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GateKeep.Api.Infrastructure.Persistence.Configurations;

public sealed class EspacioConfiguration : IEntityTypeConfiguration<Espacio>
{
    public void Configure(EntityTypeBuilder<Espacio> builder)
    {
        builder.ToTable("espacios");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        // Mapear las propiedades de la clase base
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

        builder.HasDiscriminator<string>("tipo")
            .HasValue<Espacio>("espacio")
            .HasValue<Edificio>("edificio")
            .HasValue<Laboratorio>("laboratorio")
            .HasValue<Salon>("salon");
    }
}


