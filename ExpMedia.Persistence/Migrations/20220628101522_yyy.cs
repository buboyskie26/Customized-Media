using Microsoft.EntityFrameworkCore.Migrations;

namespace ExpMedia.Persistence.Migrations
{
    public partial class yyy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFollowings_AspNetUsers_UserToFollowId",
                table: "UserFollowings");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFollowings_AspNetUsers_UserToFollowId",
                table: "UserFollowings",
                column: "UserToFollowId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFollowings_AspNetUsers_UserToFollowId",
                table: "UserFollowings");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFollowings_AspNetUsers_UserToFollowId",
                table: "UserFollowings",
                column: "UserToFollowId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
