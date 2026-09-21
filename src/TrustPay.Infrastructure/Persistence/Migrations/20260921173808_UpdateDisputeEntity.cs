using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrustPay.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDisputeEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RevokedAt",
                table: "RefreshToken",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                table: "Disputes",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RevokedAt",
                table: "RefreshToken");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Disputes");
        }
    }
}
