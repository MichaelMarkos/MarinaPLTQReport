using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace maria.Migrations
{
    /// <inheritdoc />
    public partial class init14 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeliveryNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryNotes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryReport",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ClientSignaturePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TechSignaturePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProjectAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TechName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryReport", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryNoteReport",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    deliveryNoteId = table.Column<int>(type: "int", nullable: false),
                    deliveryReportId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryNoteReport", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryNoteReport_DeliveryNotes_deliveryNoteId",
                        column: x => x.deliveryNoteId,
                        principalTable: "DeliveryNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeliveryNoteReport_DeliveryReport_deliveryReportId",
                        column: x => x.deliveryReportId,
                        principalTable: "DeliveryReport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNoteReport_deliveryNoteId",
                table: "DeliveryNoteReport",
                column: "deliveryNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNoteReport_deliveryReportId",
                table: "DeliveryNoteReport",
                column: "deliveryReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryNoteReport");

            migrationBuilder.DropTable(
                name: "DeliveryNotes");

            migrationBuilder.DropTable(
                name: "DeliveryReport");
        }
    }
}
