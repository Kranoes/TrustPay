using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrustPay.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BloomFilterMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastNickNameChangedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastNickNameChangedAt",
                table: "Users");
        }
    }
}
