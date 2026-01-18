using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace maria.Migrations
{
    /// <inheritdoc />
    public partial class WallStatusForList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WallStatusForList",
                table: "Elevator",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WallStatusForList",
                table: "Elevator");
        }
    }
}
