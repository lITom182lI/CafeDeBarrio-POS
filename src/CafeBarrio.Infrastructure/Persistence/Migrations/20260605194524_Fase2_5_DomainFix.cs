using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeBarrio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Fase2_5_DomainFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "operador_id",
                table: "Transaccion",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "recargo_propina",
                table: "Transaccion",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "turno_id",
                table: "Transaccion",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "cantidad_disponible",
                table: "Producto",
                type: "decimal(10,3)",
                precision: 10,
                scale: 3,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "seguimiento_inventario",
                table: "Producto",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<decimal>(
                name: "stock_minimo",
                table: "Producto",
                type: "decimal(10,3)",
                precision: 10,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "unidad_medida",
                table: "Producto",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "unidad");

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
                name: "Operador",
                columns: table => new
                {
                    OperadorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SedeId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PinHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
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
                    ImpactoInventario = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
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
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Abierto"),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
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
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientoCaja", x => x.MovimientoCajaId);
                    table.ForeignKey(
                        name: "FK_MovimientoCaja_Turno_TurnoId",
                        column: x => x.TurnoId,
                        principalTable: "Turno",
                        principalColumn: "TurnoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transaccion_fecha",
                table: "Transaccion",
                column: "fecha");

            migrationBuilder.CreateIndex(
                name: "IX_Transaccion_operador_id",
                table: "Transaccion",
                column: "operador_id");

            migrationBuilder.CreateIndex(
                name: "IX_Transaccion_turno_id",
                table: "Transaccion",
                column: "turno_id");

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
                name: "IX_ConfiguracionNegocio_SedeId_Activo",
                table: "ConfiguracionNegocio",
                columns: new[] { "SedeId", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoCaja_TurnoId",
                table: "MovimientoCaja",
                column: "TurnoId");

            migrationBuilder.CreateIndex(
                name: "IX_Operador_SedeId",
                table: "Operador",
                column: "SedeId");

            migrationBuilder.CreateIndex(
                name: "IX_Turno_FechaApertura",
                table: "Turno",
                column: "FechaApertura");

            migrationBuilder.CreateIndex(
                name: "IX_Turno_OperadorId",
                table: "Turno",
                column: "OperadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Turno_SedeId",
                table: "Turno",
                column: "SedeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaccion_Operador_operador_id",
                table: "Transaccion",
                column: "operador_id",
                principalTable: "Operador",
                principalColumn: "OperadorId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaccion_Turno_turno_id",
                table: "Transaccion",
                column: "turno_id",
                principalTable: "Turno",
                principalColumn: "TurnoId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaccion_Operador_operador_id",
                table: "Transaccion");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaccion_Turno_turno_id",
                table: "Transaccion");

            migrationBuilder.DropTable(
                name: "Anulacion");

            migrationBuilder.DropTable(
                name: "ConfiguracionNegocio");

            migrationBuilder.DropTable(
                name: "MovimientoCaja");

            migrationBuilder.DropTable(
                name: "Turno");

            migrationBuilder.DropTable(
                name: "Operador");

            migrationBuilder.DropIndex(
                name: "IX_Transaccion_fecha",
                table: "Transaccion");

            migrationBuilder.DropIndex(
                name: "IX_Transaccion_operador_id",
                table: "Transaccion");

            migrationBuilder.DropIndex(
                name: "IX_Transaccion_turno_id",
                table: "Transaccion");

            migrationBuilder.DropColumn(
                name: "operador_id",
                table: "Transaccion");

            migrationBuilder.DropColumn(
                name: "recargo_propina",
                table: "Transaccion");

            migrationBuilder.DropColumn(
                name: "turno_id",
                table: "Transaccion");

            migrationBuilder.DropColumn(
                name: "seguimiento_inventario",
                table: "Producto");

            migrationBuilder.DropColumn(
                name: "stock_minimo",
                table: "Producto");

            migrationBuilder.DropColumn(
                name: "unidad_medida",
                table: "Producto");

            migrationBuilder.AlterColumn<int>(
                name: "cantidad_disponible",
                table: "Producto",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,3)",
                oldPrecision: 10,
                oldScale: 3);
        }
    }
}
