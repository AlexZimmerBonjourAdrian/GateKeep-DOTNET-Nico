using GateKeep.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GateKeep.Api.Infrastructure.Persistence.Configurations;

public sealed class EstudianteConfiguration : IEntityTypeConfiguration<Estudiante>
{
    public void Configure(EntityTypeBuilder<Estudiante> builder)
    {
        builder.ToTable("estudiantes");
        
        // TPT: No configurar clave aquí, se hereda de la tabla base
        
        // Propiedades específicas de Estudiante (si las hay en el futuro)
        // Por ahora solo hereda las propiedades base
        
        // Índices específicos para estudiantes si es necesario
        // builder.HasIndex(x => x.Matricula)
        //     .HasDatabaseName("IX_estudiantes_matricula")
        //     .IsUnique();
    }
}
