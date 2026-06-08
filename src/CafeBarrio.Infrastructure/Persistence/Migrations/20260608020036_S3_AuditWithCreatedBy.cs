using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeBarrio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class S3_AuditWithCreatedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Turno",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Turno",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Turno",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Turno",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Transaccion",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Transaccion",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Producto",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Producto",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Operador",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Operador",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Operador",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Operador",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "MovimientoCaja",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "MovimientoCaja",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "MovimientoCaja",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "MovimientoCaja",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Anulacion",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Anulacion",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Anulacion",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Anulacion",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Turno");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Turno");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Turno");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Turno");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Transaccion");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Transaccion");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Producto");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Producto");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Operador");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Operador");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Operador");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Operador");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "MovimientoCaja");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "MovimientoCaja");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "MovimientoCaja");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "MovimientoCaja");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Anulacion");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Anulacion");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Anulacion");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Anulacion");
        }
    }
}
