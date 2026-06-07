using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeBarrio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMetodoPagoSecundario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "metodo_pago_secundario_id",
                table: "Transaccion",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "monto_metodo_primario",
                table: "Transaccion",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaccion_metodo_pago_secundario_id",
                table: "Transaccion",
                column: "metodo_pago_secundario_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaccion_MetodoPago_metodo_pago_secundario_id",
                table: "Transaccion",
                column: "metodo_pago_secundario_id",
                principalTable: "MetodoPago",
                principalColumn: "metodo_pago_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaccion_MetodoPago_metodo_pago_secundario_id",
                table: "Transaccion");

            migrationBuilder.DropIndex(
                name: "IX_Transaccion_metodo_pago_secundario_id",
                table: "Transaccion");

            migrationBuilder.DropColumn(
                name: "metodo_pago_secundario_id",
                table: "Transaccion");

            migrationBuilder.DropColumn(
                name: "monto_metodo_primario",
                table: "Transaccion");
        }
    }
}
