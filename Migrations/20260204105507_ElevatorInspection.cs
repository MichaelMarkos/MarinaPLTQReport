using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace maria.Migrations
{
    /// <inheritdoc />
    public partial class ElevatorInspection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ElevatorChechingItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Item = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElevatorChechingItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ElevatorInspectionReport",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Projectlocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProjectDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ReportNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientSignaturePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TechSignaturePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TechName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNum = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElevatorInspectionReport", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ElevatorInspectionItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CheckingItemId = table.Column<int>(type: "int", nullable: false),
                    ElevatorInspectionReportId = table.Column<int>(type: "int", nullable: false),
                    CorrectiveAction = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fault = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorrectiveActionFlag = table.Column<bool>(type: "bit", nullable: false),
                    faultFlag = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElevatorInspectionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ElevatorInspectionItems_ElevatorChechingItems_CheckingItemId",
                        column: x => x.CheckingItemId,
                        principalTable: "ElevatorChechingItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ElevatorInspectionItems_ElevatorInspectionReport_ElevatorInspectionReportId",
                        column: x => x.ElevatorInspectionReportId,
                        principalTable: "ElevatorInspectionReport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ElevatorInspectionItems_CheckingItemId",
                table: "ElevatorInspectionItems",
                column: "CheckingItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ElevatorInspectionItems_ElevatorInspectionReportId",
                table: "ElevatorInspectionItems",
                column: "ElevatorInspectionReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ElevatorInspectionItems");

            migrationBuilder.DropTable(
                name: "ElevatorChechingItems");

            migrationBuilder.DropTable(
                name: "ElevatorInspectionReport");
        }
    }
}
