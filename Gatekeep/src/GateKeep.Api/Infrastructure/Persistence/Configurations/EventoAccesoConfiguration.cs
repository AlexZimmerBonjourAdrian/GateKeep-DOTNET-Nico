using GateKeep.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GateKeep.Api.Infrastructure.Persistence.Configurations;

public sealed class EventoAccesoConfiguration : IEntityTypeConfiguration<EventoAcceso>
{
    public void Configure(EntityTypeBuilder<EventoAcceso> builder)
    {
        builder.ToTable("eventos_acceso");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Fecha).IsRequired();
        builder.Property(x => x.Resultado).IsRequired().HasMaxLength(100);
        builder.Property(x => x.PuntoControl).IsRequired().HasMaxLength(120);
        builder.Property(x => x.UsuarioId).IsRequired();
        builder.Property(x => x.EspacioId).IsRequired();
    }
}


