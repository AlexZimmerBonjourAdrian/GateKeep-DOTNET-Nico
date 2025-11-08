using GateKeep.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GateKeep.Api.Infrastructure.Persistence.Configurations;

public sealed class EventoConfiguration : IEntityTypeConfiguration<Evento>
{
    public void Configure(EntityTypeBuilder<Evento> builder)
    {
        builder.ToTable("eventos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Fecha).IsRequired();
        builder.Property(x => x.Resultado).IsRequired().HasMaxLength(100);
        builder.Property(x => x.PuntoControl).IsRequired().HasMaxLength(120);
        builder.Property(x => x.Activo).IsRequired().HasDefaultValue(true);

        builder.HasIndex(x => x.Fecha)
            .HasDatabaseName("IX_eventos_fecha");
    }
}

