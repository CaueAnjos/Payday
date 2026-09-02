using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaydayBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentPaidColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Paid",
                schema: "Contract",
                table: "Payments",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Paid",
                schema: "Contract",
                table: "Payments");
        }
    }
}
