using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace maria.Migrations
{
    /// <inheritdoc />
    public partial class init5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PdfFilePath",
                table: "Reports");

            migrationBuilder.AddColumn<string>(
                name: "ClientSignaturePath",
                table: "SiteReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TechSignaturePath",
                table: "SiteReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SiteReportImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    siteReportId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteReportImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SiteReportImages_SiteReports_siteReportId",
                        column: x => x.siteReportId,
                        principalTable: "SiteReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SiteReportImages_siteReportId",
                table: "SiteReportImages",
                column: "siteReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteReportImages");

            migrationBuilder.DropColumn(
                name: "ClientSignaturePath",
                table: "SiteReports");

            migrationBuilder.DropColumn(
                name: "TechSignaturePath",
                table: "SiteReports");

            migrationBuilder.AddColumn<string>(
                name: "PdfFilePath",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
