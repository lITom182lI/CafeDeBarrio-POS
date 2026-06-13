using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeBarrio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class V3_IntegrityConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Transaccion",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Transaccion_Subtotal_Positivo",
                table: "Transaccion",
                sql: "[subtotal] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Transaccion_Total_Coherente",
                table: "Transaccion",
                sql: "[total] >= [subtotal] + [impuesto]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Transaccion_Total_Positivo",
                table: "Transaccion",
                sql: "[total] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Producto_Costo_Positivo",
                table: "Producto",
                sql: "[costo] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Producto_Precio_Positivo",
                table: "Producto",
                sql: "[precio] >= 0");

            migrationBuilder.CreateIndex(
                name: "UX_Cliente_Email",
                table: "Cliente",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Transaccion_Subtotal_Positivo",
                table: "Transaccion");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Transaccion_Total_Coherente",
                table: "Transaccion");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Transaccion_Total_Positivo",
                table: "Transaccion");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Producto_Costo_Positivo",
                table: "Producto");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Producto_Precio_Positivo",
                table: "Producto");

            migrationBuilder.DropIndex(
                name: "UX_Cliente_Email",
                table: "Cliente");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Transaccion");
        }
    }
}
