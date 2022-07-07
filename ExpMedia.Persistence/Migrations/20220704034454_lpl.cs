using Microsoft.EntityFrameworkCore.Migrations;

namespace ExpMedia.Persistence.Migrations
{
    public partial class lpl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MessageTableId",
                table: "Messagesx",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Messagesx_MessageTableId",
                table: "Messagesx",
                column: "MessageTableId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messagesx_MessageTables_MessageTableId",
                table: "Messagesx",
                column: "MessageTableId",
                principalTable: "MessageTables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messagesx_MessageTables_MessageTableId",
                table: "Messagesx");

            migrationBuilder.DropIndex(
                name: "IX_Messagesx_MessageTableId",
                table: "Messagesx");

            migrationBuilder.DropColumn(
                name: "MessageTableId",
                table: "Messagesx");
        }
    }
}
