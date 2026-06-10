using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeBarrio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class S10_SunatIntentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SunatIntentos",
                table: "Transaccion",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SunatIntentos",
                table: "Transaccion");
        }
    }
}
