using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrustPay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefreshTokenStoreHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Token",
                table: "RefreshToken",
                newName: "TokenHash");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_TokenHash",
                table: "RefreshToken",
                column: "TokenHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RefreshToken_TokenHash",
                table: "RefreshToken");

            migrationBuilder.RenameColumn(
                name: "TokenHash",
                table: "RefreshToken",
                newName: "Token");
        }
    }
}
