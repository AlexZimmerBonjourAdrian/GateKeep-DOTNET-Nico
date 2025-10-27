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
        // TiposUsuarioPermitidos: guardado como texto CSV por simplicidad
        builder.Property(x => x.TiposUsuarioPermitidos)
            .HasConversion(
                v => string.Join(',', v),
                v => (IReadOnlyList<GateKeep.Api.Domain.Enums.TipoUsuario>)v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => Enum.Parse<GateKeep.Api.Domain.Enums.TipoUsuario>(s))
                        .ToList()
            );

        builder.Property(x => x.EspacioId).IsRequired();
    }
}


