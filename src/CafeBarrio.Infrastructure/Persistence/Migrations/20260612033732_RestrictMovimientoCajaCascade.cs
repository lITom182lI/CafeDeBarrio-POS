using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeBarrio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RestrictMovimientoCajaCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientoCaja_Turno_TurnoId",
                table: "MovimientoCaja");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientoCaja_Turno_TurnoId",
                table: "MovimientoCaja",
                column: "TurnoId",
                principalTable: "Turno",
                principalColumn: "TurnoId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientoCaja_Turno_TurnoId",
                table: "MovimientoCaja");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientoCaja_Turno_TurnoId",
                table: "MovimientoCaja",
                column: "TurnoId",
                principalTable: "Turno",
                principalColumn: "TurnoId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
