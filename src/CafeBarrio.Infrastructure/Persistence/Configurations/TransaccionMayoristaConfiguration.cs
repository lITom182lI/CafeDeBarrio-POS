using CafeBarrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeBarrio.Infrastructure.Persistence.Configurations;

public class TransaccionMayoristaConfiguration : IEntityTypeConfiguration<TransaccionMayorista>
{
    public void Configure(EntityTypeBuilder<TransaccionMayorista> builder)
    {
        builder.ToTable("TransaccionMayorista");
        builder.HasKey(x => x.TmId);
        builder.Property(x => x.TmId).HasColumnName("tm_id");
        builder.Property(x => x.TransaccionId).HasColumnName("transaccion_id");
        builder.Property(x => x.DescuentoPorcentaje).HasColumnName("descuento_porcentaje").HasPrecision(5, 2).IsRequired();
        builder.Property(x => x.TransporteId).HasColumnName("transporte_id");
        builder.Property(x => x.InstruccionesEntrega).HasColumnName("instrucciones_entrega");
        builder.Property(x => x.FechaEntregaEstimada).HasColumnName("fecha_entrega_estimada");

        builder.HasOne(x => x.Transaccion)
               .WithOne(t => t.TransaccionMayorista)
               .HasForeignKey<TransaccionMayorista>(x => x.TransaccionId);

        builder.HasOne(x => x.Transporte)
               .WithMany(t => t.TransaccionesMayoristas)
               .HasForeignKey(x => x.TransporteId);
    }
}
