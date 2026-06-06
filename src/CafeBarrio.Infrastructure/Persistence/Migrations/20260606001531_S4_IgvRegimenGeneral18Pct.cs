using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeBarrio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class S4_IgvRegimenGeneral18Pct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ConfiguracionNegocio",
                keyColumn: "ConfiguracionNegocioId",
                keyValue: 1,
                columns: new[] { "TasaIGV", "TasaIPM", "FechaVigencia" },
                values: new object[] { 0.16m, 0.02m, new DateTime(2026, 6, 5) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ConfiguracionNegocio",
                keyColumn: "ConfiguracionNegocioId",
                keyValue: 1,
                columns: new[] { "TasaIGV", "TasaIPM", "FechaVigencia" },
                values: new object[] { 0.08m, 0.025m, new DateTime(2026, 1, 1) });
        }
    }
}
