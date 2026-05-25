using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace maria.Migrations
{
    /// <inheritdoc />
    public partial class checkbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CheckBox",
                table: "DeliveryReport",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckBox",
                table: "DeliveryReport");
        }
    }
}
