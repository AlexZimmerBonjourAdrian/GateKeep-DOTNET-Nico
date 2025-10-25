using GateKeep.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GateKeep.Api.Infrastructure.Persistence.Configurations;

public sealed class NotificacionUsuarioConfiguration : IEntityTypeConfiguration<NotificacionUsuario>
{
    public void Configure(EntityTypeBuilder<NotificacionUsuario> builder)
    {
        builder.ToTable("notificaciones_usuarios");
        builder.HasKey(x => new { x.UsuarioId, x.NotificacionId });
        builder.Property(x => x.Leido).IsRequired();
    }
}


