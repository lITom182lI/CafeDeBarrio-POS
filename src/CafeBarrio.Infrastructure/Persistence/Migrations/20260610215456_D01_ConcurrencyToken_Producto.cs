using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeBarrio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class D01_ConcurrencyToken_Producto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fecha_actualizacion",
                table: "Producto");

            migrationBuilder.DropColumn(
                name: "fecha_creacion",
                table: "Producto");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Producto",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Producto");

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_actualizacion",
                table: "Producto",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_creacion",
                table: "Producto",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
