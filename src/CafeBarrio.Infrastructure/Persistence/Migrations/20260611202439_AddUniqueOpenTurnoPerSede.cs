using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeBarrio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueOpenTurnoPerSede : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Turno_SedeId",
                table: "Turno");

            migrationBuilder.CreateIndex(
                name: "UX_Turnos_SedeId_Abierto",
                table: "Turno",
                column: "SedeId",
                unique: true,
                filter: "[Estado] = 'Abierto'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Turnos_SedeId_Abierto",
                table: "Turno");

            migrationBuilder.CreateIndex(
                name: "IX_Turno_SedeId",
                table: "Turno",
                column: "SedeId");
        }
    }
}
