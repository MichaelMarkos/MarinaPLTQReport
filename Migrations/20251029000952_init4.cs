using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace maria.Migrations
{
    /// <inheritdoc />
    public partial class init4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CheckingItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Item = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckingItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SiteReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CheckingItemReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CheckingItemId = table.Column<int>(type: "int", nullable: false),
                    SiteReportId = table.Column<int>(type: "int", nullable: false),
                    CorrectiveAction = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fault = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckingItemReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CheckingItemReports_CheckingItems_CheckingItemId",
                        column: x => x.CheckingItemId,
                        principalTable: "CheckingItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CheckingItemReports_SiteReports_SiteReportId",
                        column: x => x.SiteReportId,
                        principalTable: "SiteReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CheckingItemReports_CheckingItemId",
                table: "CheckingItemReports",
                column: "CheckingItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckingItemReports_SiteReportId",
                table: "CheckingItemReports",
                column: "SiteReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CheckingItemReports");

            migrationBuilder.DropTable(
                name: "CheckingItems");

            migrationBuilder.DropTable(
                name: "SiteReports");
        }
    }
}
