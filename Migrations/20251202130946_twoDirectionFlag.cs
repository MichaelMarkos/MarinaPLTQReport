using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace maria.Migrations
{
    /// <inheritdoc />
    public partial class twoDirectionFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "doortwoDirections",
                table: "Elevator",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "garagstwoDirections",
                table: "Elevator",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "twoDirectionFlag",
                table: "Elevator",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "doortwoDirections",
                table: "Elevator");

            migrationBuilder.DropColumn(
                name: "garagstwoDirections",
                table: "Elevator");

            migrationBuilder.DropColumn(
                name: "twoDirectionFlag",
                table: "Elevator");
        }
    }
}
