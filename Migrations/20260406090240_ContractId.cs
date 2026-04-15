using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace maria.Migrations
{
    /// <inheritdoc />
    public partial class ContractId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "SiteReports",
                newName: "ContractId");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "SafetyReport",
                newName: "ContractId");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "Reports",
                newName: "ContractId");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "LevatorReport",
                newName: "ContractId");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "ElevatorInspectionReport",
                newName: "ContractId");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "Elevator",
                newName: "ContractId");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "DeliveryReport",
                newName: "ContractId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ContractId",
                table: "SiteReports",
                newName: "ProjectId");

            migrationBuilder.RenameColumn(
                name: "ContractId",
                table: "SafetyReport",
                newName: "ProjectId");

            migrationBuilder.RenameColumn(
                name: "ContractId",
                table: "Reports",
                newName: "ProjectId");

            migrationBuilder.RenameColumn(
                name: "ContractId",
                table: "LevatorReport",
                newName: "ProjectId");

            migrationBuilder.RenameColumn(
                name: "ContractId",
                table: "ElevatorInspectionReport",
                newName: "ProjectId");

            migrationBuilder.RenameColumn(
                name: "ContractId",
                table: "Elevator",
                newName: "ProjectId");

            migrationBuilder.RenameColumn(
                name: "ContractId",
                table: "DeliveryReport",
                newName: "ProjectId");
        }
    }
}
