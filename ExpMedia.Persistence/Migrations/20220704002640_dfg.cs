using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ExpMedia.Persistence.Migrations
{
    public partial class dfg : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MessageTo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageToUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MessageById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MessageCreated = table.Column<DateTime>(type: "datetime2", nullable: false)
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageTo");
        }
    }
}
