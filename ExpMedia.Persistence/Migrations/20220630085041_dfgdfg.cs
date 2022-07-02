using Microsoft.EntityFrameworkCore.Migrations;

namespace ExpMedia.Persistence.Migrations
{
    public partial class dfgdfg : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSeen",
                table: "UserFollowings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSeen",
                table: "ActivityNotifications",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSeen",
                table: "UserFollowings");

            migrationBuilder.DropColumn(
                name: "IsSeen",
                table: "ActivityNotifications");
        }
    }
}
