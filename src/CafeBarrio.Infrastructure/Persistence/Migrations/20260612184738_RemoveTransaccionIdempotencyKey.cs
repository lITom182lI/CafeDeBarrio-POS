using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeBarrio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTransaccionIdempotencyKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Transacciones_IdempotencyKey",
                table: "Transaccion");

            migrationBuilder.DropColumn(
                name: "idempotency_key",
                table: "Transaccion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "idempotency_key",
                table: "Transaccion",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_Transacciones_IdempotencyKey",
                table: "Transaccion",
                column: "idempotency_key",
                unique: true,
                filter: "[idempotency_key] IS NOT NULL");
        }
    }
}
