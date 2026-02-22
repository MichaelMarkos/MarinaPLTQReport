using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace maria.Migrations
{
    /// <inheritdoc />
    public partial class Levator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LevatorReport",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Projectlocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProjectDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ReportNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BuildingType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BuildingKind = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TechName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNum = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LevatorReport", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Facade",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Number = table.Column<int>(type: "int", nullable: false),
                    TypeOfFinish = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TypeOfWall = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TypeOfLand = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Height = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Width = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Max = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LevatorReportId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facade", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Facade_LevatorReport_LevatorReportId",
                        column: x => x.LevatorReportId,
                        principalTable: "LevatorReport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LevatorImage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LevatorReportId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LevatorImage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LevatorImage_LevatorReport_LevatorReportId",
                        column: x => x.LevatorReportId,
                        principalTable: "LevatorReport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Scaffold",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FacadeId = table.Column<int>(type: "int", nullable: false),
                    TypeOfUse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TypeOfGroup = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SetGroup = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TypeBox = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HeightBox = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WidthBox = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NumberTransfers = table.Column<int>(type: "int", nullable: true),
                    Wirelength = table.Column<int>(type: "int", nullable: true),
                    ElectricWirelength = table.Column<int>(type: "int", nullable: true),
                    PowerSource = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Liftingلأoods = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    x = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    y = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scaffold", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scaffold_Facade_FacadeId",
                        column: x => x.FacadeId,
                        principalTable: "Facade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Facade_LevatorReportId",
                table: "Facade",
                column: "LevatorReportId");

            migrationBuilder.CreateIndex(
                name: "IX_LevatorImage_LevatorReportId",
                table: "LevatorImage",
                column: "LevatorReportId");

            migrationBuilder.CreateIndex(
                name: "IX_Scaffold_FacadeId",
                table: "Scaffold",
                column: "FacadeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LevatorImage");

            migrationBuilder.DropTable(
                name: "Scaffold");

            migrationBuilder.DropTable(
                name: "Facade");

            migrationBuilder.DropTable(
                name: "LevatorReport");
        }
    }
}
