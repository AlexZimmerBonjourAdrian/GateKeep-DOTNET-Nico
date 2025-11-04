using GateKeep.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GateKeep.Api.Infrastructure.Persistence.Configurations;

public sealed class BeneficioUsuarioConfiguration : IEntityTypeConfiguration<BeneficioUsuario>
{
    public void Configure(EntityTypeBuilder<BeneficioUsuario> builder)
    {
        builder.ToTable("beneficios_usuarios");
        builder.HasKey(x => new { x.UsuarioId, x.BeneficioId });
        builder.Property(x => x.EstadoCanje).IsRequired();
    }
}


