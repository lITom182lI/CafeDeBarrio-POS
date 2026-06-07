using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeBarrio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transaccion_sede_id",
                table: "Transaccion");

            migrationBuilder.CreateIndex(
                name: "IX_Transaccion_SedeId_Fecha",
                table: "Transaccion",
                columns: new[] { "sede_id", "fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_Producto_Activo",
                table: "Producto",
                column: "activo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transaccion_SedeId_Fecha",
                table: "Transaccion");

            migrationBuilder.DropIndex(
                name: "IX_Producto_Activo",
                table: "Producto");

            migrationBuilder.CreateIndex(
                name: "IX_Transaccion_sede_id",
                table: "Transaccion",
                column: "sede_id");
        }
    }
}
