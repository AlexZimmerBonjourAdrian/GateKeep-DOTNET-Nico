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

        // Índices para optimizar consultas por foreign keys
        builder.HasIndex(x => x.UsuarioId)
            .HasDatabaseName("IX_eventos_acceso_usuario_id");
            
        builder.HasIndex(x => x.EspacioId)
            .HasDatabaseName("IX_eventos_acceso_espacio_id");
            
        builder.HasIndex(x => x.Fecha)
            .HasDatabaseName("IX_eventos_acceso_fecha");

        // Relación One-to-Many: Usuario-EventoAcceso
        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación One-to-Many: Espacio-EventoAcceso
        builder.HasOne<Espacio>()
            .WithMany()
            .HasForeignKey(x => x.EspacioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}


