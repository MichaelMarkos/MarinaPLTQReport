using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace maria.Migrations
{
    /// <inheritdoc />
    public partial class safteyReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InvoiceNumber",
                table: "SiteReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Elevator",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ReportNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProjectAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    resizableSquarewidth = table.Column<int>(type: "int", nullable: false),
                    resizableSquareHeight = table.Column<int>(type: "int", nullable: false),
                    typeElevator = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    shapeType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    widthShape = table.Column<int>(type: "int", nullable: true),
                    heightShape = table.Column<int>(type: "int", nullable: true),
                    radiusShape = table.Column<int>(type: "int", nullable: true),
                    directionShape = table.Column<int>(type: "int", nullable: false),
                    floors = table.Column<int>(type: "int", nullable: false),
                    foundationHeight = table.Column<int>(type: "int", nullable: false),
                    floorHeights = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    workRequied = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClientName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClientSignaturePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TechSignaturePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TechName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNum = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Elevator", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SafetyItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Item = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SafetyItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SafetyReport",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ReportNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TeamNum = table.Column<int>(type: "int", nullable: false),
                    TeamLeaderNum = table.Column<int>(type: "int", nullable: false),
                    TeamLeaderName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProjectDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TeamMembers = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClientSignaturePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TechSignaturePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProjectName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SiteName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Projectlocation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TechName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNum = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SafetyReport", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ElevatorImage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ElevatorId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElevatorImage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ElevatorImage_Elevator_ElevatorId",
                        column: x => x.ElevatorId,
                        principalTable: "Elevator",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SafetyItemsReport",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SafetyItemsId = table.Column<int>(type: "int", nullable: false),
                    SafetyReportId = table.Column<int>(type: "int", nullable: false),
                    CorrectiveAction = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    faultFlag = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SafetyItemsReport", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SafetyItemsReport_SafetyItems_SafetyItemsId",
                        column: x => x.SafetyItemsId,
                        principalTable: "SafetyItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SafetyItemsReport_SafetyReport_SafetyReportId",
                        column: x => x.SafetyReportId,
                        principalTable: "SafetyReport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SafetyReportImage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    safetyReportId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SafetyReportImage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SafetyReportImage_SafetyReport_safetyReportId",
                        column: x => x.safetyReportId,
                        principalTable: "SafetyReport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ElevatorImage_ElevatorId",
                table: "ElevatorImage",
                column: "ElevatorId");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyItemsReport_SafetyItemsId",
                table: "SafetyItemsReport",
                column: "SafetyItemsId");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyItemsReport_SafetyReportId",
                table: "SafetyItemsReport",
                column: "SafetyReportId");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyReportImage_safetyReportId",
                table: "SafetyReportImage",
                column: "safetyReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ElevatorImage");

            migrationBuilder.DropTable(
                name: "SafetyItemsReport");

            migrationBuilder.DropTable(
                name: "SafetyReportImage");

            migrationBuilder.DropTable(
                name: "Elevator");

            migrationBuilder.DropTable(
                name: "SafetyItems");

            migrationBuilder.DropTable(
                name: "SafetyReport");

            migrationBuilder.DropColumn(
                name: "InvoiceNumber",
                table: "SiteReports");
        }
    }
}
