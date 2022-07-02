using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ExpMedia.Persistence.Migrations
{
    public partial class yy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlockUsersx",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserToBlockId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UserWhoBlockId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    BlockCreation = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockUsersx", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlockUsersx_AspNetUsers_UserToBlockId",
                        column: x => x.UserToBlockId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BlockUsersx_AspNetUsers_UserWhoBlockId",
                        column: x => x.UserWhoBlockId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlockUsersx_UserToBlockId",
                table: "BlockUsersx",
                column: "UserToBlockId");

            migrationBuilder.CreateIndex(
                name: "IX_BlockUsersx_UserWhoBlockId",
                table: "BlockUsersx",
                column: "UserWhoBlockId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlockUsersx");
        }
    }
}
