using Microsoft.EntityFrameworkCore.Migrations;

namespace ExpMedia.Persistence.Migrations
{
    public partial class yjh : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActivityUserId",
                table: "Comments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ActivityUserId",
                table: "Comments",
                column: "ActivityUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_AspNetUsers_ActivityUserId",
                table: "Comments",
                column: "ActivityUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_AspNetUsers_ActivityUserId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_ActivityUserId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "ActivityUserId",
                table: "Comments");
        }
    }
}
