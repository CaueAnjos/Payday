using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaydayBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentContractLinkAndDropPaidColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ContractId",
                schema: "Contract",
                table: "Payments",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ContractId",
                schema: "Contract",
                table: "Payments",
                column: "ContractId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Contracts_ContractId",
                schema: "Contract",
                table: "Payments",
                column: "ContractId",
                principalSchema: "Contract",
                principalTable: "Contracts",
                principalColumn: "Id");

            // The `Paid` column was added by AddPaymentPaidColumn, then the property
            // was removed from the Payment model (in favor of tracking "paid" via
            // Payment.Signature) without ever dropping the column itself - FixContract
            // was supposed to do it but ended up being a no-op. Different databases
            // may or may not actually still have this column (ours locally doesn't,
            // despite AddPaymentPaidColumn being recorded as applied), so clean it up
            // idempotently rather than assuming it is there.
            migrationBuilder.Sql(
                """ALTER TABLE "Contract"."Payments" DROP COLUMN IF EXISTS "Paid";"""
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE "Contract"."Payments"
                ADD COLUMN IF NOT EXISTS "Paid" boolean NOT NULL DEFAULT false;
                """
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Contracts_ContractId",
                schema: "Contract",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_ContractId",
                schema: "Contract",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ContractId",
                schema: "Contract",
                table: "Payments");
        }
    }
}
