using Microsoft.EntityFrameworkCore.Migrations;

namespace ExpMedia.Persistence.Migrations
{
    public partial class hnbw : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MessageTables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageToUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MessageById = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageTables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageTables_AspNetUsers_MessageById",
                        column: x => x.MessageById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MessageTables_AspNetUsers_MessageToUserId",
                        column: x => x.MessageToUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessageTables_MessageById",
                table: "MessageTables",
                column: "MessageById");

            migrationBuilder.CreateIndex(
                name: "IX_MessageTables_MessageToUserId",
                table: "MessageTables",
                column: "MessageToUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageTables");
        }
    }
}
