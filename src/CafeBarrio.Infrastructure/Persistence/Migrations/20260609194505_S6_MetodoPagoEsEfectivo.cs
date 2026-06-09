using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeBarrio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class S6_MetodoPagoEsEfectivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EsEfectivo",
                table: "MetodoPago",
                type: "bit",
                nullable: false,
                defaultValue: false);
            
            migrationBuilder.Sql("UPDATE MetodoPago SET EsEfectivo = 1 WHERE Nombre = 'Efectivo'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EsEfectivo",
                table: "MetodoPago");
        }
    }
}
