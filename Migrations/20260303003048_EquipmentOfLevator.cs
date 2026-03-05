using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace maria.Migrations
{
    /// <inheritdoc />
    public partial class EquipmentOfLevator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Scaffold",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecialText",
                table: "Scaffold",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EquipmentOfLevator",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FacadeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentOfLevator", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipmentOfLevator_Facade_FacadeId",
                        column: x => x.FacadeId,
                        principalTable: "Facade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "facadeImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FacadeId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_facadeImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_facadeImages_Facade_FacadeId",
                        column: x => x.FacadeId,
                        principalTable: "Facade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentOfLevator_FacadeId",
                table: "EquipmentOfLevator",
                column: "FacadeId");

            migrationBuilder.CreateIndex(
                name: "IX_facadeImages_FacadeId",
                table: "facadeImages",
                column: "FacadeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EquipmentOfLevator");

            migrationBuilder.DropTable(
                name: "facadeImages");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "Scaffold");

            migrationBuilder.DropColumn(
                name: "SpecialText",
                table: "Scaffold");
        }
    }
}
