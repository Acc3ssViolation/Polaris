using Microsoft.EntityFrameworkCore.Migrations;

namespace Polaris.Migrations
{
    public partial class SimplifiedPermissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowedOperations",
                table: "Permissions");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AllowedOperations",
                table: "Permissions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
