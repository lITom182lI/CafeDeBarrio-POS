using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeBarrio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategoriaCafe",
                columns: table => new
                {
                    categoria_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    codigo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    activa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriaCafe", x => x.categoria_id);
                });

            migrationBuilder.CreateTable(
                name: "MetodoPago",
                columns: table => new
                {
                    metodo_pago_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetodoPago", x => x.metodo_pago_id);
                });

            migrationBuilder.CreateTable(
                name: "OpcionEnvio",
                columns: table => new
                {
                    opcion_envio_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    tarifa = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    activa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpcionEnvio", x => x.opcion_envio_id);
                });

            migrationBuilder.CreateTable(
                name: "Sede",
                columns: table => new
                {
                    sede_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    direccion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    distrito = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ciudad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    es_principal = table.Column<bool>(type: "bit", nullable: false),
                    activa = table.Column<bool>(type: "bit", nullable: false),
                    fecha_apertura = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sede", x => x.sede_id);
                });

            migrationBuilder.CreateTable(
                name: "TipoCliente",
                columns: table => new
                {
                    tipo_cliente_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoCliente", x => x.tipo_cliente_id);
                });

            migrationBuilder.CreateTable(
                name: "Transporte",
                columns: table => new
                {
                    transporte_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    placa = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    capacidad_kg = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    disponible = table.Column<bool>(type: "bit", nullable: false),
                    observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transporte", x => x.transporte_id);
                });

            migrationBuilder.CreateTable(
                name: "UbicacionPreferencia",
                columns: table => new
                {
                    ubicacion_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UbicacionPreferencia", x => x.ubicacion_id);
                });

            migrationBuilder.CreateTable(
                name: "Producto",
                columns: table => new
                {
                    producto_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    categoria_id = table.Column<int>(type: "int", nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    costo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    precio = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    cantidad_disponible = table.Column<int>(type: "int", nullable: false),
                    cantidad_por_unidad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    imagen_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    es_mayorista = table.Column<bool>(type: "bit", nullable: false),
                    activo = table.Column<bool>(type: "bit", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fecha_actualizacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Producto", x => x.producto_id);
                    table.ForeignKey(
                        name: "FK_Producto_CategoriaCafe_categoria_id",
                        column: x => x.categoria_id,
                        principalTable: "CategoriaCafe",
                        principalColumn: "categoria_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cliente",
                columns: table => new
                {
                    cliente_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tipo_cliente_id = table.Column<int>(type: "int", nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    apellido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    codigo_cliente = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    tipo_documento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    numero_documento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    direccion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    distrito = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ciudad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ubicacion_id = table.Column<int>(type: "int", nullable: true),
                    categoria_pref_id = table.Column<int>(type: "int", nullable: true),
                    fecha_registro = table.Column<DateOnly>(type: "date", nullable: false),
                    activo = table.Column<bool>(type: "bit", nullable: false),
                    observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cliente", x => x.cliente_id);
                    table.ForeignKey(
                        name: "FK_Cliente_CategoriaCafe_categoria_pref_id",
                        column: x => x.categoria_pref_id,
                        principalTable: "CategoriaCafe",
                        principalColumn: "categoria_id");
                    table.ForeignKey(
                        name: "FK_Cliente_TipoCliente_tipo_cliente_id",
                        column: x => x.tipo_cliente_id,
                        principalTable: "TipoCliente",
                        principalColumn: "tipo_cliente_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cliente_UbicacionPreferencia_ubicacion_id",
                        column: x => x.ubicacion_id,
                        principalTable: "UbicacionPreferencia",
                        principalColumn: "ubicacion_id");
                });

            migrationBuilder.CreateTable(
                name: "Transaccion",
                columns: table => new
                {
                    transaccion_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cliente_id = table.Column<int>(type: "int", nullable: false),
                    sede_id = table.Column<int>(type: "int", nullable: false),
                    metodo_pago_id = table.Column<int>(type: "int", nullable: false),
                    opcion_envio_id = table.Column<int>(type: "int", nullable: true),
                    es_mayorista = table.Column<bool>(type: "bit", nullable: false),
                    canal = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    impuesto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    costo_envio = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    notas = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaccion", x => x.transaccion_id);
                    table.ForeignKey(
                        name: "FK_Transaccion_Cliente_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "Cliente",
                        principalColumn: "cliente_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transaccion_MetodoPago_metodo_pago_id",
                        column: x => x.metodo_pago_id,
                        principalTable: "MetodoPago",
                        principalColumn: "metodo_pago_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transaccion_OpcionEnvio_opcion_envio_id",
                        column: x => x.opcion_envio_id,
                        principalTable: "OpcionEnvio",
                        principalColumn: "opcion_envio_id");
                    table.ForeignKey(
                        name: "FK_Transaccion_Sede_sede_id",
                        column: x => x.sede_id,
                        principalTable: "Sede",
                        principalColumn: "sede_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetalleTransaccion",
                columns: table => new
                {
                    detalle_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    transaccion_id = table.Column<int>(type: "int", nullable: false),
                    producto_id = table.Column<int>(type: "int", nullable: false),
                    cantidad = table.Column<int>(type: "int", nullable: false),
                    precio_unitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    subtotal_linea = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleTransaccion", x => x.detalle_id);
                    table.ForeignKey(
                        name: "FK_DetalleTransaccion_Producto_producto_id",
                        column: x => x.producto_id,
                        principalTable: "Producto",
                        principalColumn: "producto_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetalleTransaccion_Transaccion_transaccion_id",
                        column: x => x.transaccion_id,
                        principalTable: "Transaccion",
                        principalColumn: "transaccion_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransaccionMayorista",
                columns: table => new
                {
                    tm_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    transaccion_id = table.Column<int>(type: "int", nullable: false),
                    descuento_porcentaje = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    transporte_id = table.Column<int>(type: "int", nullable: true),
                    instrucciones_entrega = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fecha_entrega_estimada = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransaccionMayorista", x => x.tm_id);
                    table.ForeignKey(
                        name: "FK_TransaccionMayorista_Transaccion_transaccion_id",
                        column: x => x.transaccion_id,
                        principalTable: "Transaccion",
                        principalColumn: "transaccion_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransaccionMayorista_Transporte_transporte_id",
                        column: x => x.transporte_id,
                        principalTable: "Transporte",
                        principalColumn: "transporte_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_categoria_pref_id",
                table: "Cliente",
                column: "categoria_pref_id");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_tipo_cliente_id",
                table: "Cliente",
                column: "tipo_cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_ubicacion_id",
                table: "Cliente",
                column: "ubicacion_id");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleTransaccion_producto_id",
                table: "DetalleTransaccion",
                column: "producto_id");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleTransaccion_transaccion_id",
                table: "DetalleTransaccion",
                column: "transaccion_id");

            migrationBuilder.CreateIndex(
                name: "IX_Producto_categoria_id",
                table: "Producto",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "IX_Transaccion_cliente_id",
                table: "Transaccion",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_Transaccion_metodo_pago_id",
                table: "Transaccion",
                column: "metodo_pago_id");

            migrationBuilder.CreateIndex(
                name: "IX_Transaccion_opcion_envio_id",
                table: "Transaccion",
                column: "opcion_envio_id");

            migrationBuilder.CreateIndex(
                name: "IX_Transaccion_sede_id",
                table: "Transaccion",
                column: "sede_id");

            migrationBuilder.CreateIndex(
                name: "IX_TransaccionMayorista_transaccion_id",
                table: "TransaccionMayorista",
                column: "transaccion_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransaccionMayorista_transporte_id",
                table: "TransaccionMayorista",
                column: "transporte_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetalleTransaccion");

            migrationBuilder.DropTable(
                name: "TransaccionMayorista");

            migrationBuilder.DropTable(
                name: "Producto");

            migrationBuilder.DropTable(
                name: "Transaccion");

            migrationBuilder.DropTable(
                name: "Transporte");

            migrationBuilder.DropTable(
                name: "Cliente");

            migrationBuilder.DropTable(
                name: "MetodoPago");

            migrationBuilder.DropTable(
                name: "OpcionEnvio");

            migrationBuilder.DropTable(
                name: "Sede");

            migrationBuilder.DropTable(
                name: "CategoriaCafe");

            migrationBuilder.DropTable(
                name: "TipoCliente");

            migrationBuilder.DropTable(
                name: "UbicacionPreferencia");
        }
    }
}
