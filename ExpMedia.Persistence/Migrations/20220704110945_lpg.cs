using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ExpMedia.Persistence.Migrations
{
    public partial class lpg : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MessagesGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UserMadeById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GroupCreation = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessagesGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessagesGroups_AspNetUsers_MessageById",
                        column: x => x.MessageById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubMessageGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Body = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MessageToUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MessageCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MessagesGroupId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubMessageGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubMessageGroups_AspNetUsers_MessageToUserId",
                        column: x => x.MessageToUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubMessageGroups_MessagesGroups_MessagesGroupId",
                        column: x => x.MessagesGroupId,
                        principalTable: "MessagesGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessagesGroups_MessageById",
                table: "MessagesGroups",
                column: "MessageById");

            migrationBuilder.CreateIndex(
                name: "IX_SubMessageGroups_MessagesGroupId",
                table: "SubMessageGroups",
                column: "MessagesGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SubMessageGroups_MessageToUserId",
                table: "SubMessageGroups",
                column: "MessageToUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubMessageGroups");

            migrationBuilder.DropTable(
                name: "MessagesGroups");
        }
    }
}
