using CafeBarrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeBarrio.Infrastructure.Persistence.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Cliente");
        builder.HasKey(x => x.ClienteId);
        builder.Property(x => x.ClienteId).HasColumnName("cliente_id");
        builder.Property(x => x.TipoClienteId).HasColumnName("tipo_cliente_id");
        builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Apellido).HasColumnName("apellido").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
        builder.Property(x => x.CodigoCliente).HasColumnName("codigo_cliente").HasMaxLength(20);
        builder.Property(x => x.TipoDocumento).HasColumnName("tipo_documento").HasMaxLength(20);
        builder.Property(x => x.NumeroDocumento).HasColumnName("numero_documento").HasMaxLength(50);
        builder.Property(x => x.Telefono).HasColumnName("telefono").HasMaxLength(20);
        builder.Property(x => x.Direccion).HasColumnName("direccion").HasMaxLength(300);
        builder.Property(x => x.Distrito).HasColumnName("distrito").HasMaxLength(100);
        builder.Property(x => x.Ciudad).HasColumnName("ciudad").HasMaxLength(100);
        builder.Property(x => x.UbicacionId).HasColumnName("ubicacion_id");
        builder.Property(x => x.CategoriaPrefId).HasColumnName("categoria_pref_id");
        builder.Property(x => x.FechaRegistro).HasColumnName("fecha_registro").IsRequired();
        builder.Property(x => x.Activo).HasColumnName("activo").IsRequired();
        builder.Property(x => x.Observaciones).HasColumnName("observaciones");

        builder.HasOne(x => x.TipoCliente)
               .WithMany(t => t.Clientes)
               .HasForeignKey(x => x.TipoClienteId);

        builder.HasOne(x => x.Ubicacion)
               .WithMany(u => u.Clientes)
               .HasForeignKey(x => x.UbicacionId);

        builder.HasOne(x => x.CategoriaPreferida)
               .WithMany(c => c.ClientesConPreferencia)
               .HasForeignKey(x => x.CategoriaPrefId);
    }
}
