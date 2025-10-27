using GateKeep.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GateKeep.Api.Infrastructure.Persistence.Configurations;

public sealed class AdminConfiguration : IEntityTypeConfiguration<Admin>
{
    public void Configure(EntityTypeBuilder<Admin> builder)
    {
        builder.ToTable("admins");
        
        // TPT: No configurar clave aquí, se hereda de la tabla base
        
        // Propiedades específicas de Admin (si las hay en el futuro)
        // Por ahora solo hereda las propiedades base
        
        // Índices específicos para admins si es necesario
        // builder.HasIndex(x => x.NivelAcceso)
        //     .HasDatabaseName("IX_admins_nivel_acceso");
    }
}
