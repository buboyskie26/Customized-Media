using Microsoft.EntityFrameworkCore.Migrations;

namespace ExpMedia.Persistence.Migrations
{
    public partial class dfga : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CommentCreatedUserId",
                table: "CommentReactions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommentReactions_CommentCreatedUserId",
                table: "CommentReactions",
                column: "CommentCreatedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommentReactions_AspNetUsers_CommentCreatedUserId",
                table: "CommentReactions",
                column: "CommentCreatedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommentReactions_AspNetUsers_CommentCreatedUserId",
                table: "CommentReactions");

            migrationBuilder.DropIndex(
                name: "IX_CommentReactions_CommentCreatedUserId",
                table: "CommentReactions");

            migrationBuilder.DropColumn(
                name: "CommentCreatedUserId",
                table: "CommentReactions");
        }
    }
}
