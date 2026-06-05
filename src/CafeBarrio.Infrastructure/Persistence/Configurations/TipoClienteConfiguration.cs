using CafeBarrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeBarrio.Infrastructure.Persistence.Configurations;

public class TipoClienteConfiguration : IEntityTypeConfiguration<TipoCliente>
{
    public void Configure(EntityTypeBuilder<TipoCliente> builder)
    {
        builder.ToTable("TipoCliente");
        builder.HasKey(x => x.TipoClienteId);
        builder.Property(x => x.TipoClienteId).HasColumnName("tipo_cliente_id");
        builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(200);
    }
}
