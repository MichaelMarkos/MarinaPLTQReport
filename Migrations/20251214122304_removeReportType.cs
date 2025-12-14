using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace maria.Migrations
{
    /// <inheritdoc />
    public partial class removeReportType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReportType",
                table: "Reports");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReportType",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
