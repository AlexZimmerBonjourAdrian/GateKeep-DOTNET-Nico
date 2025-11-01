using GateKeep.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GateKeep.Api.Infrastructure.Persistence.Configurations;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        // Propiedades comunes de la clase base
        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(256);
            
        builder.Property(x => x.Nombre)
            .IsRequired()
            .HasMaxLength(120);
            
        builder.Property(x => x.Apellido)
            .IsRequired()
            .HasMaxLength(120);
            
        builder.Property(x => x.Contrasenia)
            .IsRequired()
            .HasMaxLength(256);
            
        builder.Property(x => x.Telefono)
            .HasMaxLength(64);
            
        builder.Property(x => x.FechaAlta)
            .IsRequired();

        builder.Property(x => x.Credencial)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Rol)
            .HasConversion<int>()
            .IsRequired();

        // Índices para optimizar consultas comunes
        builder.HasIndex(x => x.Email)
            .IsUnique()
            .HasDatabaseName("IX_usuarios_email");
            
        builder.HasIndex(x => x.Credencial)
            .HasDatabaseName("IX_usuarios_credencial");

        builder.HasIndex(x => x.Rol)
            .HasDatabaseName("IX_usuarios_rol");
    }
}


