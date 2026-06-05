using CafeBarrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeBarrio.Infrastructure.Persistence.Configurations;

public class DetalleTransaccionConfiguration : IEntityTypeConfiguration<DetalleTransaccion>
{
    public void Configure(EntityTypeBuilder<DetalleTransaccion> builder)
    {
        builder.ToTable("DetalleTransaccion");
        builder.HasKey(x => x.DetalleId);
        builder.Property(x => x.DetalleId).HasColumnName("detalle_id");
        builder.Property(x => x.TransaccionId).HasColumnName("transaccion_id");
        builder.Property(x => x.ProductoId).HasColumnName("producto_id");
        builder.Property(x => x.Cantidad).HasColumnName("cantidad").IsRequired();
        builder.Property(x => x.PrecioUnitario).HasColumnName("precio_unitario").HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.SubtotalLinea).HasColumnName("subtotal_linea").HasPrecision(18, 2).IsRequired();

        builder.HasOne(x => x.Transaccion)
               .WithMany(t => t.Detalles)
               .HasForeignKey(x => x.TransaccionId);

        builder.HasOne(x => x.Producto)
               .WithMany(p => p.Detalles)
               .HasForeignKey(x => x.ProductoId);
    }
}
