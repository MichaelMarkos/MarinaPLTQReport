using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace maria.Migrations
{
    /// <inheritdoc />
    public partial class changeintableelevator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "capinaHeight",
                table: "Elevator",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "capinaStatus",
                table: "Elevator",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "centerWidth",
                table: "Elevator",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "directionHeight",
                table: "Elevator",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "directionWidth",
                table: "Elevator",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "liftWidth",
                table: "Elevator",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "rightWidth",
                table: "Elevator",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "salesName",
                table: "Elevator",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "wellStatus",
                table: "Elevator",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "capinaHeight",
                table: "Elevator");

            migrationBuilder.DropColumn(
                name: "capinaStatus",
                table: "Elevator");

            migrationBuilder.DropColumn(
                name: "centerWidth",
                table: "Elevator");

            migrationBuilder.DropColumn(
                name: "directionHeight",
                table: "Elevator");

            migrationBuilder.DropColumn(
                name: "directionWidth",
                table: "Elevator");

            migrationBuilder.DropColumn(
                name: "liftWidth",
                table: "Elevator");

            migrationBuilder.DropColumn(
                name: "rightWidth",
                table: "Elevator");

            migrationBuilder.DropColumn(
                name: "salesName",
                table: "Elevator");

            migrationBuilder.DropColumn(
                name: "wellStatus",
                table: "Elevator");
        }
    }
}
