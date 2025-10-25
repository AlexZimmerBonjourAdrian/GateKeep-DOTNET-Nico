using GateKeep.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GateKeep.Api.Infrastructure.Persistence.Configurations;

public sealed class EspacioConfiguration : IEntityTypeConfiguration<Espacio>
{
    public void Configure(EntityTypeBuilder<Espacio> builder)
    {
        builder.ToTable("espacios");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.HasDiscriminator<string>("tipo")
            .HasValue<Espacio>("espacio")
            .HasValue<Edificio>("edificio")
            .HasValue<Laboratorio>("laboratorio")
            .HasValue<Salon>("salon");
    }
}


