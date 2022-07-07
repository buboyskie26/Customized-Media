using Microsoft.EntityFrameworkCore.Migrations;

namespace ExpMedia.Persistence.Migrations
{
    public partial class hni : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messagesx_AspNetUsers_MessageToUserId",
                table: "Messagesx");

            migrationBuilder.DropIndex(
                name: "IX_Messagesx_MessageToUserId",
                table: "Messagesx");

            migrationBuilder.DropColumn(
                name: "MessageToUserId",
                table: "Messagesx");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MessageToUserId",
                table: "Messagesx",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messagesx_MessageToUserId",
                table: "Messagesx",
                column: "MessageToUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messagesx_AspNetUsers_MessageToUserId",
                table: "Messagesx",
                column: "MessageToUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
