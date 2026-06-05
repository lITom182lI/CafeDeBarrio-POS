using CafeBarrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeBarrio.Infrastructure.Persistence.Configurations;

public class UbicacionPreferenciaConfiguration : IEntityTypeConfiguration<UbicacionPreferencia>
{
    public void Configure(EntityTypeBuilder<UbicacionPreferencia> builder)
    {
        builder.ToTable("UbicacionPreferencia");
        builder.HasKey(x => x.UbicacionId);
        builder.Property(x => x.UbicacionId).HasColumnName("ubicacion_id");
        builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(200);
    }
}
