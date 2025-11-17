using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractMonthlyClaimSystem.Migrations
{
    /// <inheritdoc />
    public partial class FifthMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Claims_ClaimID1",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_ClaimID1",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ClaimID1",
                table: "Documents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClaimID1",
                table: "Documents",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ClaimID1",
                table: "Documents",
                column: "ClaimID1");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Claims_ClaimID1",
                table: "Documents",
                column: "ClaimID1",
                principalTable: "Claims",
                principalColumn: "ClaimID");
        }
    }
}
