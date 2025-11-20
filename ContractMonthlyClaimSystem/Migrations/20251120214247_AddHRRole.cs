using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractMonthlyClaimSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddHRRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "RoleID", "RoleName" },
                values: new object[] { 4, "HR" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "RoleID",
                keyValue: 4);
        }
    }
}
