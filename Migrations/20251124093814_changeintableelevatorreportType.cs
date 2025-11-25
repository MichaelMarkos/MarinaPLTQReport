using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace maria.Migrations
{
    /// <inheritdoc />
    public partial class changeintableelevatorreportType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientName",
                table: "Elevator");

            migrationBuilder.DropColumn(
                name: "ClientSignaturePath",
                table: "Elevator");

            migrationBuilder.RenameColumn(
                name: "TechSignaturePath",
                table: "Elevator",
                newName: "reportType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "reportType",
                table: "Elevator",
                newName: "TechSignaturePath");

            migrationBuilder.AddColumn<string>(
                name: "ClientName",
                table: "Elevator",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ClientSignaturePath",
                table: "Elevator",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
