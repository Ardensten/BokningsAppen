using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BokningsAppen.Migrations
{
    public partial class smallchange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "SocialSecurityNumber",
                table: "Users",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "SocialSecurityNumber",
                table: "Users",
                type: "float",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}
