using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrustPay.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWalletUserForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_wallets_Users_UserId",
                table: "wallets");

            migrationBuilder.AddColumn<Guid>(
                name: "AuthorId",
                table: "Reviews",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TargetUserId",
                table: "Reviews",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_AuthorId",
                table: "Reviews",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_TargetUserId",
                table: "Reviews",
                column: "TargetUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Users_AuthorId",
                table: "Reviews",
                column: "AuthorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Users_TargetUserId",
                table: "Reviews",
                column: "TargetUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Users_AuthorId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Users_TargetUserId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_AuthorId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_TargetUserId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "TargetUserId",
                table: "Reviews");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "orders",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddForeignKey(
                name: "FK_wallets_Users_UserId",
                table: "wallets",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
