using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeBarrio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class S7_TransaccionSunatEstado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SunatError",
                table: "Transaccion",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SunatEstado",
                table: "Transaccion",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE Transaccion SET SunatEstado = 'Emitida' WHERE SunatEstado IS NULL OR SunatEstado = ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SunatError",
                table: "Transaccion");

            migrationBuilder.DropColumn(
                name: "SunatEstado",
                table: "Transaccion");
        }
    }
}
