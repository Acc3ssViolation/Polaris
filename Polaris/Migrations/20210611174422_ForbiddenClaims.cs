using Microsoft.EntityFrameworkCore.Migrations;

namespace Polaris.Migrations
{
    public partial class ForbiddenClaims : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Allow",
                table: "Permissions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Allow",
                table: "Permissions");
        }
    }
}
