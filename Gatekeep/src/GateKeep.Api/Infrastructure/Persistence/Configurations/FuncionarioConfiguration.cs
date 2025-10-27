using GateKeep.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GateKeep.Api.Infrastructure.Persistence.Configurations;

public sealed class FuncionarioConfiguration : IEntityTypeConfiguration<Funcionario>
{
    public void Configure(EntityTypeBuilder<Funcionario> builder)
    {
        builder.ToTable("funcionarios");
        
        // TPT: No configurar clave aquí, se hereda de la tabla base
        
        // Propiedades específicas de Funcionario (si las hay en el futuro)
        // Por ahora solo hereda las propiedades base
        
        // Índices específicos para funcionarios si es necesario
        // builder.HasIndex(x => x.NumeroEmpleado)
        //     .HasDatabaseName("IX_funcionarios_numero_empleado")
        //     .IsUnique();
    }
}
