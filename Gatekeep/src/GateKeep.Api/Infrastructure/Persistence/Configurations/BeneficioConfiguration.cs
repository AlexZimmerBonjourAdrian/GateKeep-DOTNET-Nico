using GateKeep.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GateKeep.Api.Infrastructure.Persistence.Configurations;

public sealed class BeneficioConfiguration : IEntityTypeConfiguration<Beneficio>
{
    public void Configure(EntityTypeBuilder<Beneficio> builder)
    {
        builder.ToTable("beneficios");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Tipo).HasConversion<int>().IsRequired();
        builder.Property(x => x.Vigencia).IsRequired();
        builder.Property(x => x.FechaDeVencimiento).IsRequired();
        builder.Property(x => x.Cupos).IsRequired();
    }
}


