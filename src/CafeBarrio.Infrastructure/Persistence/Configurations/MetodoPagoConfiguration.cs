using CafeBarrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeBarrio.Infrastructure.Persistence.Configurations;

public class MetodoPagoConfiguration : IEntityTypeConfiguration<MetodoPago>
{
    public void Configure(EntityTypeBuilder<MetodoPago> builder)
    {
        builder.ToTable("MetodoPago");
        builder.HasKey(x => x.MetodoPagoId);
        builder.Property(x => x.MetodoPagoId).HasColumnName("metodo_pago_id");
        builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Activo).HasColumnName("activo").IsRequired();
    }
}
