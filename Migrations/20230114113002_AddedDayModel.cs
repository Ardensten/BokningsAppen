using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BokningsAppen.Migrations
{
    public partial class AddedDayModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Day",
                table: "Bookings");

            migrationBuilder.AddColumn<int>(
                name: "DayId",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DayId",
                table: "Bookings");

            migrationBuilder.AddColumn<string>(
                name: "Day",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
