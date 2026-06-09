using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeBarrio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes_Ref07 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Turno_OperadorId",
                table: "Turno",
                newName: "IX_Turnos_OperadorId");

            migrationBuilder.RenameIndex(
                name: "IX_Turno_FechaApertura",
                table: "Turno",
                newName: "IX_Turnos_FechaApertura");

            migrationBuilder.RenameIndex(
                name: "IX_Transaccion_turno_id",
                table: "Transaccion",
                newName: "IX_Transacciones_TurnoId");

            migrationBuilder.RenameIndex(
                name: "IX_Transaccion_operador_id",
                table: "Transaccion",
                newName: "IX_Transacciones_OperadorId");

            migrationBuilder.RenameIndex(
                name: "IX_Transaccion_fecha",
                table: "Transaccion",
                newName: "IX_Transacciones_Fecha");

            migrationBuilder.RenameIndex(
                name: "IX_Producto_categoria_id",
                table: "Producto",
                newName: "IX_Productos_CategoriaId");

            migrationBuilder.RenameIndex(
                name: "IX_Producto_Activo",
                table: "Producto",
                newName: "IX_Productos_Activo");

            migrationBuilder.CreateIndex(
                name: "IX_Transacciones_Fecha_OperadorId",
                table: "Transaccion",
                columns: new[] { "fecha", "operador_id" });

            migrationBuilder.CreateIndex(
                name: "IX_Operadores_Activo",
                table: "Operador",
                column: "Activo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transacciones_Fecha_OperadorId",
                table: "Transaccion");

            migrationBuilder.DropIndex(
                name: "IX_Operadores_Activo",
                table: "Operador");

            migrationBuilder.RenameIndex(
                name: "IX_Turnos_OperadorId",
                table: "Turno",
                newName: "IX_Turno_OperadorId");

            migrationBuilder.RenameIndex(
                name: "IX_Turnos_FechaApertura",
                table: "Turno",
                newName: "IX_Turno_FechaApertura");

            migrationBuilder.RenameIndex(
                name: "IX_Transacciones_TurnoId",
                table: "Transaccion",
                newName: "IX_Transaccion_turno_id");

            migrationBuilder.RenameIndex(
                name: "IX_Transacciones_OperadorId",
                table: "Transaccion",
                newName: "IX_Transaccion_operador_id");

            migrationBuilder.RenameIndex(
                name: "IX_Transacciones_Fecha",
                table: "Transaccion",
                newName: "IX_Transaccion_fecha");

            migrationBuilder.RenameIndex(
                name: "IX_Productos_CategoriaId",
                table: "Producto",
                newName: "IX_Producto_categoria_id");

            migrationBuilder.RenameIndex(
                name: "IX_Productos_Activo",
                table: "Producto",
                newName: "IX_Producto_Activo");
        }
    }
}
