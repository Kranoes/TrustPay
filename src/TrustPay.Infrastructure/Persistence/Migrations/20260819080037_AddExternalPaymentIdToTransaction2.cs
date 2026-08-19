using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrustPay.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalPaymentIdToTransaction2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_wallets_ReceiverWalletId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_wallets_SenderWalletId",
                table: "Transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions");

            migrationBuilder.RenameTable(
                name: "Transactions",
                newName: "transactions");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_SenderWalletId",
                table: "transactions",
                newName: "IX_transactions_SenderWalletId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_ReceiverWalletId",
                table: "transactions",
                newName: "IX_transactions_ReceiverWalletId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_CreatedAt",
                table: "transactions",
                newName: "IX_transactions_CreatedAt");

            migrationBuilder.AddPrimaryKey(
                name: "PK_transactions",
                table: "transactions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_transactions_wallets_ReceiverWalletId",
                table: "transactions",
                column: "ReceiverWalletId",
                principalTable: "wallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_transactions_wallets_SenderWalletId",
                table: "transactions",
                column: "SenderWalletId",
                principalTable: "wallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_transactions_wallets_ReceiverWalletId",
                table: "transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_transactions_wallets_SenderWalletId",
                table: "transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_transactions",
                table: "transactions");

            migrationBuilder.RenameTable(
                name: "transactions",
                newName: "Transactions");

            migrationBuilder.RenameIndex(
                name: "IX_transactions_SenderWalletId",
                table: "Transactions",
                newName: "IX_Transactions_SenderWalletId");

            migrationBuilder.RenameIndex(
                name: "IX_transactions_ReceiverWalletId",
                table: "Transactions",
                newName: "IX_Transactions_ReceiverWalletId");

            migrationBuilder.RenameIndex(
                name: "IX_transactions_CreatedAt",
                table: "Transactions",
                newName: "IX_Transactions_CreatedAt");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_wallets_ReceiverWalletId",
                table: "Transactions",
                column: "ReceiverWalletId",
                principalTable: "wallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_wallets_SenderWalletId",
                table: "Transactions",
                column: "SenderWalletId",
                principalTable: "wallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
