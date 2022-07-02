using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ExpMedia.Persistence.Migrations
{
    public partial class efvg : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    NotificationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NotifyToId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NotifyFromId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityNotifications_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActivityNotifications_AspNetUsers_NotifyFromId",
                        column: x => x.NotifyFromId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivityNotifications_AspNetUsers_NotifyToId",
                        column: x => x.NotifyToId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityNotifications_ActivityId",
                table: "ActivityNotifications",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityNotifications_NotifyFromId",
                table: "ActivityNotifications",
                column: "NotifyFromId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityNotifications_NotifyToId",
                table: "ActivityNotifications",
                column: "NotifyToId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityNotifications");
        }
    }
}
