using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ExpMedia.Persistence.Migrations
{
    public partial class hnb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageTo");

            migrationBuilder.CreateTable(
                name: "Messagesx",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Body = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MessageToUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MessageById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MessageCreated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messagesx", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messagesx_AspNetUsers_MessageById",
                        column: x => x.MessageById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messagesx_AspNetUsers_MessageToUserId",
                        column: x => x.MessageToUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messagesx_MessageById",
                table: "Messagesx",
                column: "MessageById");

            migrationBuilder.CreateIndex(
                name: "IX_Messagesx_MessageToUserId",
                table: "Messagesx",
                column: "MessageToUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messagesx");

            migrationBuilder.CreateTable(
                name: "MessageTo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Body = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MessageById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MessageCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MessageToUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageTo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageTo_AspNetUsers_MessageById",
                        column: x => x.MessageById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MessageTo_AspNetUsers_MessageToUserId",
                        column: x => x.MessageToUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessageTo_MessageById",
                table: "MessageTo",
                column: "MessageById");

            migrationBuilder.CreateIndex(
                name: "IX_MessageTo_MessageToUserId",
                table: "MessageTo",
                column: "MessageToUserId");
        }
    }
}
