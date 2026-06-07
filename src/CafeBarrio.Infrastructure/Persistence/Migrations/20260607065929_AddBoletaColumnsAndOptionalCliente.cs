using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeBarrio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBoletaColumnsAndOptionalCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaccion_Cliente_cliente_id",
                table: "Transaccion");

            migrationBuilder.AlterColumn<int>(
                name: "cliente_id",
                table: "Transaccion",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "canal",
                table: "Transaccion",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "POS",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "numero_documento",
                table: "Transaccion",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "razon_social",
                table: "Transaccion",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tipo_documento",
                table: "Transaccion",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaccion_Cliente_cliente_id",
                table: "Transaccion",
                column: "cliente_id",
                principalTable: "Cliente",
                principalColumn: "cliente_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaccion_Cliente_cliente_id",
                table: "Transaccion");

            migrationBuilder.DropColumn(
                name: "numero_documento",
                table: "Transaccion");

            migrationBuilder.DropColumn(
                name: "razon_social",
                table: "Transaccion");

            migrationBuilder.DropColumn(
                name: "tipo_documento",
                table: "Transaccion");

            migrationBuilder.AlterColumn<int>(
                name: "cliente_id",
                table: "Transaccion",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "canal",
                table: "Transaccion",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "POS");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaccion_Cliente_cliente_id",
                table: "Transaccion",
                column: "cliente_id",
                principalTable: "Cliente",
                principalColumn: "cliente_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
