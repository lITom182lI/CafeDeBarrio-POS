using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeBarrio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AUD04_UniqueActiveConfigPerSede : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UX_ConfiguracionNegocio_SedeId_Activa",
                table: "ConfiguracionNegocio",
                column: "SedeId",
                unique: true,
                filter: "[Activo] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_ConfiguracionNegocio_SedeId_Activa",
                table: "ConfiguracionNegocio");
        }
    }
}
