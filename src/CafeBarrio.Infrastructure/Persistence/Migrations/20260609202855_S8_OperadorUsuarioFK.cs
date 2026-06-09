using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeBarrio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class S8_OperadorUsuarioFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "Operador",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Operador_UsuarioId",
                table: "Operador",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Operador_Usuarios_UsuarioId",
                table: "Operador",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "UsuarioId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Operador_Usuarios_UsuarioId",
                table: "Operador");

            migrationBuilder.DropIndex(
                name: "IX_Operador_UsuarioId",
                table: "Operador");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Operador");
        }
    }
}
