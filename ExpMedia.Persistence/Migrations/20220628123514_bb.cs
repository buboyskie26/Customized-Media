using Microsoft.EntityFrameworkCore.Migrations;

namespace ExpMedia.Persistence.Migrations
{
    public partial class bb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserWhoTaggedId",
                table: "TagUsers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TagUsers_UserWhoTaggedId",
                table: "TagUsers",
                column: "UserWhoTaggedId");

            migrationBuilder.AddForeignKey(
                name: "FK_TagUsers_AspNetUsers_UserWhoTaggedId",
                table: "TagUsers",
                column: "UserWhoTaggedId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TagUsers_AspNetUsers_UserWhoTaggedId",
                table: "TagUsers");

            migrationBuilder.DropIndex(
                name: "IX_TagUsers_UserWhoTaggedId",
                table: "TagUsers");

            migrationBuilder.DropColumn(
                name: "UserWhoTaggedId",
                table: "TagUsers");
        }
    }
}
