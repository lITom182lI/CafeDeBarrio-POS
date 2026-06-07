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
        builder.Property(x => x.MetodoPagoSecundarioId).HasColumnName("metodo_pago_secundario_id");
        builder.Property(x => x.MontoMetodoPrimario).HasColumnName("monto_metodo_primario").HasPrecision(18, 2);
        builder.Property(x => x.OpcionEnvioId).HasColumnName("opcion_envio_id");
        builder.Property(x => x.TurnoId).HasColumnName("turno_id");
        builder.Property(x => x.OperadorId).HasColumnName("operador_id");
        builder.Property(x => x.EsMayorista).HasColumnName("es_mayorista").IsRequired();
        builder.Property(x => x.Canal).HasColumnName("canal").HasMaxLength(20).HasDefaultValue("POS").IsRequired();
        builder.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(x => x.Subtotal).HasColumnName("subtotal").HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Impuesto).HasColumnName("impuesto").HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.RecargoPropina).HasColumnName("recargo_propina").HasPrecision(10, 2).HasDefaultValue(0m);
        builder.Property(x => x.CostoEnvio).HasColumnName("costo_envio").HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Total).HasColumnName("total").HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Notas).HasColumnName("notas");

        // ── Boleta nominada ──────────────────────────────────────────────
        builder.Property(x => x.TipoDocumento).HasColumnName("tipo_documento").HasMaxLength(20);
        builder.Property(x => x.NumeroDocumento).HasColumnName("numero_documento").HasMaxLength(20);
        builder.Property(x => x.RazonSocial).HasColumnName("razon_social").HasMaxLength(150);

        // ── Relaciones ───────────────────────────────────────────────────
        builder.HasOne(x => x.Cliente)
               .WithMany(c => c.Transacciones)
               .HasForeignKey(x => x.ClienteId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Sede)
               .WithMany(s => s.Transacciones)
               .HasForeignKey(x => x.SedeId);

        builder.HasOne(x => x.MetodoPago)
               .WithMany(m => m.Transacciones)
               .HasForeignKey(x => x.MetodoPagoId);

        builder.HasOne(x => x.MetodoPagoSecundario)
               .WithMany()
               .HasForeignKey(x => x.MetodoPagoSecundarioId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.OpcionEnvio)
               .WithMany(o => o.Transacciones)
               .HasForeignKey(x => x.OpcionEnvioId);

        builder.HasOne(x => x.Turno)
               .WithMany(t => t.Transacciones)
               .HasForeignKey(x => x.TurnoId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Operador)
               .WithMany(o => o.Transacciones)
               .HasForeignKey(x => x.OperadorId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Fecha);
        builder.HasIndex(x => x.TurnoId);
        builder.HasIndex(x => new { x.SedeId, x.Fecha }).HasDatabaseName("IX_Transaccion_SedeId_Fecha");
    }
}
