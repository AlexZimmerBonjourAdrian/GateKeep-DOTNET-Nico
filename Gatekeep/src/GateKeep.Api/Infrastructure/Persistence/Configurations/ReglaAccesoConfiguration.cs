using GateKeep.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GateKeep.Api.Infrastructure.Persistence.Configurations;

public sealed class ReglaAccesoConfiguration : IEntityTypeConfiguration<ReglaAcceso>
{
    public void Configure(EntityTypeBuilder<ReglaAcceso> builder)
    {
        builder.ToTable("reglas_acceso");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.HorarioApertura).IsRequired();
        builder.Property(x => x.HorarioCierre).IsRequired();
        builder.Property(x => x.VigenciaApertura).IsRequired();
        builder.Property(x => x.VigenciaCierre).IsRequired();
        // RolesPermitidos: guardado como texto CSV por simplicidad
        builder.Property(x => x.RolesPermitidos)
            .HasConversion(
                v => string.Join(',', v),
                v => (IReadOnlyList<GateKeep.Api.Domain.Enums.Rol>)v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => Enum.Parse<GateKeep.Api.Domain.Enums.Rol>(s))
                        .ToList()
            );

        builder.Property(x => x.EspacioId).IsRequired();

        // Índice para optimizar consultas por EspacioId
        builder.HasIndex(x => x.EspacioId)
            .HasDatabaseName("IX_reglas_acceso_espacio_id");

        // Relación One-to-Many: Espacio-ReglaAcceso
        builder.HasOne<Espacio>()
            .WithMany()
            .HasForeignKey(x => x.EspacioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}


