using CafeBarrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeBarrio.Infrastructure.Persistence.Configurations;

public class TransaccionConfiguration : IEntityTypeConfiguration<Transaccion>
{
    public void Configure(EntityTypeBuilder<Transaccion> builder)
    {
        builder.ToTable("Transaccion");
        builder.HasKey(x => x.TransaccionId);
        builder.Property(x => x.TransaccionId).HasColumnName("transaccion_id");
        builder.Property(x => x.ClienteId).HasColumnName("cliente_id");
        builder.Property(x => x.SedeId).HasColumnName("sede_id");
        builder.Property(x => x.MetodoPagoId).HasColumnName("metodo_pago_id");
        builder.Property(x => x.OpcionEnvioId).HasColumnName("opcion_envio_id");
        builder.Property(x => x.EsMayorista).HasColumnName("es_mayorista").IsRequired();
        builder.Property(x => x.Canal).HasColumnName("canal").HasMaxLength(20).IsRequired();
        builder.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(x => x.Subtotal).HasColumnName("subtotal").HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Impuesto).HasColumnName("impuesto").HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.CostoEnvio).HasColumnName("costo_envio").HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Total).HasColumnName("total").HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Notas).HasColumnName("notas");

        builder.HasOne(x => x.Cliente)
               .WithMany(c => c.Transacciones)
               .HasForeignKey(x => x.ClienteId);

        builder.HasOne(x => x.Sede)
               .WithMany(s => s.Transacciones)
               .HasForeignKey(x => x.SedeId);

        builder.HasOne(x => x.MetodoPago)
               .WithMany(m => m.Transacciones)
               .HasForeignKey(x => x.MetodoPagoId);

        builder.HasOne(x => x.OpcionEnvio)
               .WithMany(o => o.Transacciones)
               .HasForeignKey(x => x.OpcionEnvioId);
    }
}
