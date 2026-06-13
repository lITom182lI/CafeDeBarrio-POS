using CafeBarrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeBarrio.Infrastructure.Persistence.Configurations;

public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.ToTable("Producto");
        builder.HasKey(x => x.ProductoId);
        builder.Property(x => x.ProductoId).HasColumnName("producto_id");
        builder.Property(x => x.CategoriaId).HasColumnName("categoria_id");
        builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion");
        builder.Property(x => x.Costo).HasColumnName("costo").HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Precio).HasColumnName("precio").HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.CantidadDisponible).HasColumnName("cantidad_disponible").HasPrecision(10, 3).IsRequired();
        builder.Property(x => x.StockMinimo).HasColumnName("stock_minimo").HasPrecision(10, 3).HasDefaultValue(0m);
        builder.Property(x => x.UnidadMedida).HasColumnName("unidad_medida").HasMaxLength(20).HasDefaultValue("unidad");
        builder.Property(x => x.SeguimientoInventario).HasColumnName("seguimiento_inventario").HasDefaultValue(true);
        builder.Property(x => x.CantidadPorUnidad).HasColumnName("cantidad_por_unidad").HasMaxLength(50);
        builder.Property(x => x.ImagenUrl).HasColumnName("imagen_url").HasMaxLength(500);
        builder.Property(x => x.EsMayorista).HasColumnName("es_mayorista").IsRequired();
        builder.Property(x => x.Activo).HasColumnName("activo").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("fecha_creacion").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("fecha_actualizacion");
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasOne(x => x.Categoria)
               .WithMany(c => c.Productos)
               .HasForeignKey(x => x.CategoriaId);

        builder.HasIndex(x => x.Activo).HasDatabaseName("IX_Producto_Activo");

        // V3-04: invariantes de precio a nivel SQL
        builder.HasCheckConstraint("CK_Producto_Precio_Positivo", "[precio] >= 0");
        builder.HasCheckConstraint("CK_Producto_Costo_Positivo",  "[costo] >= 0");
    }
}
