using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace maria.Migrations
{
    /// <inheritdoc />
    public partial class init13 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CorrectiveActionFlag",
                table: "CheckingItemReports",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "faultFlag",
                table: "CheckingItemReports",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorrectiveActionFlag",
                table: "CheckingItemReports");

            migrationBuilder.DropColumn(
                name: "faultFlag",
                table: "CheckingItemReports");
        }
    }
}
