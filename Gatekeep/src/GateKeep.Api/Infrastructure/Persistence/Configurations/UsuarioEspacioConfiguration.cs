using GateKeep.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GateKeep.Api.Infrastructure.Persistence.Configurations;

public sealed class UsuarioEspacioConfiguration : IEntityTypeConfiguration<UsuarioEspacio>
{
    public void Configure(EntityTypeBuilder<UsuarioEspacio> builder)
    {
        builder.ToTable("usuarios_espacios");
        builder.HasKey(x => new { x.UsuarioId, x.EspacioId });
    }
}


