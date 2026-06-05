using CafeBarrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeBarrio.Infrastructure.Persistence.Configurations;

public class TransporteConfiguration : IEntityTypeConfiguration<Transporte>
{
    public void Configure(EntityTypeBuilder<Transporte> builder)
    {
        builder.ToTable("Transporte");
        builder.HasKey(x => x.TransporteId);
        builder.Property(x => x.TransporteId).HasColumnName("transporte_id");
        builder.Property(x => x.Placa).HasColumnName("placa").HasMaxLength(20).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(200);
        builder.Property(x => x.CapacidadKg).HasColumnName("capacidad_kg").HasPrecision(10, 2);
        builder.Property(x => x.Disponible).HasColumnName("disponible").IsRequired();
        builder.Property(x => x.Observaciones).HasColumnName("observaciones");
    }
}
