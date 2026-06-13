using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeBarrio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate_Baseline : Migration
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
                name: "IdempotencyRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TransaccionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdempotencyRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MetodoPago",
                columns: table => new
                {
                    metodo_pago_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    activo = table.Column<bool>(type: "bit", nullable: false),
                    EsEfectivo = table.Column<bool>(type: "bit", nullable: false)
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
                name: "Usuarios",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    LockedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.UsuarioId);
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
                    cantidad_disponible = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false),
                    stock_minimo = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false, defaultValue: 0m),
                    unidad_medida = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "unidad"),
                    seguimiento_inventario = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    cantidad_por_unidad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    imagen_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    es_mayorista = table.Column<bool>(type: "bit", nullable: false),
                    activo = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Producto", x => x.producto_id);
                    table.CheckConstraint("CK_Producto_Costo_Positivo", "[costo] >= 0");
                    table.CheckConstraint("CK_Producto_Precio_Positivo", "[precio] >= 0");
                    table.ForeignKey(
                        name: "FK_Producto_CategoriaCafe_categoria_id",
                        column: x => x.categoria_id,
                        principalTable: "CategoriaCafe",
                        principalColumn: "categoria_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionNegocio",
                columns: table => new
                {
                    ConfiguracionNegocioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SedeId = table.Column<int>(type: "int", nullable: false),
                    TasaIGV = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    TasaIPM = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    FechaVigencia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionNegocio", x => x.ConfiguracionNegocioId);
                    table.ForeignKey(
                        name: "FK_ConfiguracionNegocio_Sede_SedeId",
                        column: x => x.SedeId,
                        principalTable: "Sede",
                        principalColumn: "sede_id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "Operador",
                columns: table => new
                {
                    OperadorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SedeId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PinHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Eliminado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    FailedPinAttempts = table.Column<int>(type: "int", nullable: false),
                    IsLockedOut = table.Column<bool>(type: "bit", nullable: false),
                    LockedUntilUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operador", x => x.OperadorId);
                    table.ForeignKey(
                        name: "FK_Operador_Sede_SedeId",
                        column: x => x.SedeId,
                        principalTable: "Sede",
                        principalColumn: "sede_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Operador_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Turno",
                columns: table => new
                {
                    TurnoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SedeId = table.Column<int>(type: "int", nullable: false),
                    OperadorId = table.Column<int>(type: "int", nullable: false),
                    FechaApertura = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MontoApertura = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    MontoEfectivoCierto = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    TotalEfectivoSistema = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    TotalVentasEfectivo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAnulacionesEfectivo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalMovimientosEntrada = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalMovimientosSalida = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SaldoEsperado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Diferencia = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Abierto"),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turno", x => x.TurnoId);
                    table.ForeignKey(
                        name: "FK_Turno_Operador_OperadorId",
                        column: x => x.OperadorId,
                        principalTable: "Operador",
                        principalColumn: "OperadorId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Turno_Sede_SedeId",
                        column: x => x.SedeId,
                        principalTable: "Sede",
                        principalColumn: "sede_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovimientoCaja",
                columns: table => new
                {
                    MovimientoCajaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TurnoId = table.Column<int>(type: "int", nullable: false),
                    TipoMovimiento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientoCaja", x => x.MovimientoCajaId);
                    table.ForeignKey(
                        name: "FK_MovimientoCaja_Turno_TurnoId",
                        column: x => x.TurnoId,
                        principalTable: "Turno",
                        principalColumn: "TurnoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transaccion",
                columns: table => new
                {
                    transaccion_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cliente_id = table.Column<int>(type: "int", nullable: true),
                    sede_id = table.Column<int>(type: "int", nullable: false),
                    metodo_pago_id = table.Column<int>(type: "int", nullable: false),
                    metodo_pago_secundario_id = table.Column<int>(type: "int", nullable: true),
                    monto_metodo_primario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    opcion_envio_id = table.Column<int>(type: "int", nullable: true),
                    turno_id = table.Column<int>(type: "int", nullable: true),
                    operador_id = table.Column<int>(type: "int", nullable: true),
                    es_mayorista = table.Column<bool>(type: "bit", nullable: false),
                    canal = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "POS"),
                    fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    impuesto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    recargo_propina = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false, defaultValue: 0m),
                    costo_envio = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    notas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tipo_documento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    numero_documento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    razon_social = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SunatEstado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SunatError = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SunatNumeroSerie = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SunatIntentos = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaccion", x => x.transaccion_id);
                    table.CheckConstraint("CK_Transaccion_Subtotal_Positivo", "[subtotal] >= 0");
                    table.CheckConstraint("CK_Transaccion_Total_Coherente", "[total] >= [subtotal] + [impuesto]");
                    table.CheckConstraint("CK_Transaccion_Total_Positivo", "[total] >= 0");
                    table.ForeignKey(
                        name: "FK_Transaccion_Cliente_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "Cliente",
                        principalColumn: "cliente_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Transaccion_MetodoPago_metodo_pago_id",
                        column: x => x.metodo_pago_id,
                        principalTable: "MetodoPago",
                        principalColumn: "metodo_pago_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transaccion_MetodoPago_metodo_pago_secundario_id",
                        column: x => x.metodo_pago_secundario_id,
                        principalTable: "MetodoPago",
                        principalColumn: "metodo_pago_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transaccion_OpcionEnvio_opcion_envio_id",
                        column: x => x.opcion_envio_id,
                        principalTable: "OpcionEnvio",
                        principalColumn: "opcion_envio_id");
                    table.ForeignKey(
                        name: "FK_Transaccion_Operador_operador_id",
                        column: x => x.operador_id,
                        principalTable: "Operador",
                        principalColumn: "OperadorId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transaccion_Sede_sede_id",
                        column: x => x.sede_id,
                        principalTable: "Sede",
                        principalColumn: "sede_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transaccion_Turno_turno_id",
                        column: x => x.turno_id,
                        principalTable: "Turno",
                        principalColumn: "TurnoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Anulacion",
                columns: table => new
                {
                    AnulacionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransaccionId = table.Column<int>(type: "int", nullable: false),
                    TipoAnulacion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MontoOriginal = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    MontoDevuelto = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    MetodoDevolucion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OperadorSolicitanteId = table.Column<int>(type: "int", nullable: false),
                    AutorizadorId = table.Column<int>(type: "int", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ImpactoInventario = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anulacion", x => x.AnulacionId);
                    table.ForeignKey(
                        name: "FK_Anulacion_Operador_AutorizadorId",
                        column: x => x.AutorizadorId,
                        principalTable: "Operador",
                        principalColumn: "OperadorId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Anulacion_Operador_OperadorSolicitanteId",
                        column: x => x.OperadorSolicitanteId,
                        principalTable: "Operador",
                        principalColumn: "OperadorId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Anulacion_Transaccion_TransaccionId",
                        column: x => x.TransaccionId,
                        principalTable: "Transaccion",
                        principalColumn: "transaccion_id",
                        onDelete: ReferentialAction.Restrict);
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
                    subtotal_linea = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                name: "IX_Anulacion_AutorizadorId",
                table: "Anulacion",
                column: "AutorizadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Anulacion_OperadorSolicitanteId",
                table: "Anulacion",
                column: "OperadorSolicitanteId");

            migrationBuilder.CreateIndex(
                name: "IX_Anulacion_TransaccionId",
                table: "Anulacion",
                column: "TransaccionId",
                unique: true);

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
                name: "UX_Cliente_Email",
                table: "Cliente",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionNegocio_SedeId_Activo",
                table: "ConfiguracionNegocio",
                columns: new[] { "SedeId", "Activo" });

            migrationBuilder.CreateIndex(
                name: "UX_ConfiguracionNegocio_SedeId_Activa",
                table: "ConfiguracionNegocio",
                column: "SedeId",
                unique: true,
                filter: "[Activo] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleTransaccion_producto_id",
                table: "DetalleTransaccion",
                column: "producto_id");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleTransaccion_transaccion_id",
                table: "DetalleTransaccion",
                column: "transaccion_id");

            migrationBuilder.CreateIndex(
                name: "UX_IdempotencyRecords_Key",
                table: "IdempotencyRecords",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoCaja_TurnoId",
                table: "MovimientoCaja",
                column: "TurnoId");

            migrationBuilder.CreateIndex(
                name: "IX_Operador_SedeId",
                table: "Operador",
                column: "SedeId");

            migrationBuilder.CreateIndex(
                name: "IX_Operador_UsuarioId",
                table: "Operador",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Operadores_Activo",
                table: "Operador",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Activo",
                table: "Producto",
                column: "activo");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CategoriaId",
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
                name: "IX_Transaccion_metodo_pago_secundario_id",
                table: "Transaccion",
                column: "metodo_pago_secundario_id");

            migrationBuilder.CreateIndex(
                name: "IX_Transaccion_opcion_envio_id",
                table: "Transaccion",
                column: "opcion_envio_id");

            migrationBuilder.CreateIndex(
                name: "IX_Transaccion_SedeId_Fecha",
                table: "Transaccion",
                columns: new[] { "sede_id", "fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_Transacciones_Fecha",
                table: "Transaccion",
                column: "fecha");

            migrationBuilder.CreateIndex(
                name: "IX_Transacciones_Fecha_OperadorId",
                table: "Transaccion",
                columns: new[] { "fecha", "operador_id" });

            migrationBuilder.CreateIndex(
                name: "IX_Transacciones_OperadorId",
                table: "Transaccion",
                column: "operador_id");

            migrationBuilder.CreateIndex(
                name: "IX_Transacciones_TurnoId",
                table: "Transaccion",
                column: "turno_id");

            migrationBuilder.CreateIndex(
                name: "IX_TransaccionMayorista_transaccion_id",
                table: "TransaccionMayorista",
                column: "transaccion_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransaccionMayorista_transporte_id",
                table: "TransaccionMayorista",
                column: "transporte_id");

            migrationBuilder.CreateIndex(
                name: "IX_Turnos_FechaApertura",
                table: "Turno",
                column: "FechaApertura");

            migrationBuilder.CreateIndex(
                name: "IX_Turnos_OperadorId",
                table: "Turno",
                column: "OperadorId");

            migrationBuilder.CreateIndex(
                name: "UX_Turnos_SedeId_Abierto",
                table: "Turno",
                column: "SedeId",
                unique: true,
                filter: "[Estado] = 'Abierto'");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Anulacion");

            migrationBuilder.DropTable(
                name: "ConfiguracionNegocio");

            migrationBuilder.DropTable(
                name: "DetalleTransaccion");

            migrationBuilder.DropTable(
                name: "IdempotencyRecords");

            migrationBuilder.DropTable(
                name: "MovimientoCaja");

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
                name: "Turno");

            migrationBuilder.DropTable(
                name: "CategoriaCafe");

            migrationBuilder.DropTable(
                name: "TipoCliente");

            migrationBuilder.DropTable(
                name: "UbicacionPreferencia");

            migrationBuilder.DropTable(
                name: "Operador");

            migrationBuilder.DropTable(
                name: "Sede");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
