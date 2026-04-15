using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace maria.Migrations
{
    /// <inheritdoc />
    public partial class ProjectId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "SiteReports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "SafetyReport",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "Reports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "LevatorReport",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "ElevatorInspectionReport",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "Elevator",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "DeliveryReport",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "SiteReports");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "SafetyReport");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "LevatorReport");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "ElevatorInspectionReport");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Elevator");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "DeliveryReport");
        }
    }
}
