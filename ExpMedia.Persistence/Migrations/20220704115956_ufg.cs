using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ExpMedia.Persistence.Migrations
{
    public partial class ufg : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessagesGroups_AspNetUsers_MessageById",
                table: "MessagesGroups");

            migrationBuilder.DropIndex(
                name: "IX_MessagesGroups_MessageById",
                table: "MessagesGroups");

            migrationBuilder.DropColumn(
                name: "Body",
                table: "SubMessageGroups");

            migrationBuilder.DropColumn(
                name: "MessageCreated",
                table: "SubMessageGroups");

            migrationBuilder.DropColumn(
                name: "MessageById",
                table: "MessagesGroups");

            migrationBuilder.AlterColumn<string>(
                name: "UserMadeById",
                table: "MessagesGroups",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "SubUserMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    SubMessageGroupId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubUserMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubUserMessages_SubMessageGroups_SubMessageGroupId",
                        column: x => x.SubMessageGroupId,
                        principalTable: "SubMessageGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessagesGroups_UserMadeById",
                table: "MessagesGroups",
                column: "UserMadeById");

            migrationBuilder.CreateIndex(
                name: "IX_SubUserMessages_SubMessageGroupId",
                table: "SubUserMessages",
                column: "SubMessageGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_MessagesGroups_AspNetUsers_UserMadeById",
                table: "MessagesGroups",
                column: "UserMadeById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessagesGroups_AspNetUsers_UserMadeById",
                table: "MessagesGroups");

            migrationBuilder.DropTable(
                name: "SubUserMessages");

            migrationBuilder.DropIndex(
                name: "IX_MessagesGroups_UserMadeById",
                table: "MessagesGroups");

            migrationBuilder.AddColumn<string>(
                name: "Body",
                table: "SubMessageGroups",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MessageCreated",
                table: "SubMessageGroups",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "UserMadeById",
                table: "MessagesGroups",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MessageById",
                table: "MessagesGroups",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessagesGroups_MessageById",
                table: "MessagesGroups",
                column: "MessageById");

            migrationBuilder.AddForeignKey(
                name: "FK_MessagesGroups_AspNetUsers_MessageById",
                table: "MessagesGroups",
                column: "MessageById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
