using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace maria.Migrations
{
    /// <inheritdoc />
    public partial class PeriodicMaintanceImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PeriodicMaintanceImage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PeriodicMaintanceReportId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodicMaintanceImage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeriodicMaintanceImage_PeriodicMaintanceReport_PeriodicMaintanceReportId",
                        column: x => x.PeriodicMaintanceReportId,
                        principalTable: "PeriodicMaintanceReport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicMaintanceImage_PeriodicMaintanceReportId",
                table: "PeriodicMaintanceImage",
                column: "PeriodicMaintanceReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PeriodicMaintanceImage");
        }
    }
}
