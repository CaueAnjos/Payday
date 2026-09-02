using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PaydayBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddSignatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payers_Contracts_ContractId",
                schema: "Contract",
                table: "Payers");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Contracts_ContractId",
                schema: "Contract",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_ContractId",
                schema: "Contract",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payers_ContractId",
                schema: "Contract",
                table: "Payers");

            migrationBuilder.DropColumn(
                name: "ContractId",
                schema: "Contract",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Paid",
                schema: "Contract",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ContractId",
                schema: "Contract",
                table: "Payers");

            migrationBuilder.EnsureSchema(
                name: "Signatures");

            migrationBuilder.AddColumn<int>(
                name: "SignatureId",
                schema: "Contract",
                table: "Payments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CloseDate",
                schema: "Contract",
                table: "Contracts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "State",
                schema: "Contract",
                table: "Contracts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ContractPayer",
                schema: "Contract",
                columns: table => new
                {
                    ContractsId = table.Column<int>(type: "integer", nullable: false),
                    ParticipantsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractPayer", x => new { x.ContractsId, x.ParticipantsId });
                    table.ForeignKey(
                        name: "FK_ContractPayer_Contracts_ContractsId",
                        column: x => x.ContractsId,
                        principalSchema: "Contract",
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContractPayer_Payers_ParticipantsId",
                        column: x => x.ParticipantsId,
                        principalSchema: "Contract",
                        principalTable: "Payers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contracts",
                schema: "Signatures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
                    ContractId = table.Column<int>(type: "integer", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contracts_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalSchema: "Contract",
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Contracts_Payers_OwnerId",
                        column: x => x.OwnerId,
                        principalSchema: "Contract",
                        principalTable: "Payers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SignatureId",
                schema: "Contract",
                table: "Payments",
                column: "SignatureId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractPayer_ParticipantsId",
                schema: "Contract",
                table: "ContractPayer",
                column: "ParticipantsId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ContractId",
                schema: "Signatures",
                table: "Contracts",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_OwnerId",
                schema: "Signatures",
                table: "Contracts",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Contracts_SignatureId",
                schema: "Contract",
                table: "Payments",
                column: "SignatureId",
                principalSchema: "Signatures",
                principalTable: "Contracts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Contracts_SignatureId",
                schema: "Contract",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "ContractPayer",
                schema: "Contract");

            migrationBuilder.DropTable(
                name: "Contracts",
                schema: "Signatures");

            migrationBuilder.DropIndex(
                name: "IX_Payments_SignatureId",
                schema: "Contract",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "SignatureId",
                schema: "Contract",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CloseDate",
                schema: "Contract",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "State",
                schema: "Contract",
                table: "Contracts");

            migrationBuilder.AddColumn<int>(
                name: "ContractId",
                schema: "Contract",
                table: "Payments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Paid",
                schema: "Contract",
                table: "Payments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ContractId",
                schema: "Contract",
                table: "Payers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ContractId",
                schema: "Contract",
                table: "Payments",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Payers_ContractId",
                schema: "Contract",
                table: "Payers",
                column: "ContractId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payers_Contracts_ContractId",
                schema: "Contract",
                table: "Payers",
                column: "ContractId",
                principalSchema: "Contract",
                principalTable: "Contracts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Contracts_ContractId",
                schema: "Contract",
                table: "Payments",
                column: "ContractId",
                principalSchema: "Contract",
                principalTable: "Contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
