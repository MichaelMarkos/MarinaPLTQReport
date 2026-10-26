using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace maria.Migrations
{
    /// <inheritdoc />
    public partial class ElevatorSpecification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ElevatorSpecification",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractId = table.Column<int>(type: "int", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ReportNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    reportType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    salesName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProjectAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    resizableSquarewidth = table.Column<int>(type: "int", nullable: false),
                    resizableSquareHeight = table.Column<int>(type: "int", nullable: false),
                    typeElevator = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    shapeType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    widthShape = table.Column<int>(type: "int", nullable: true),
                    heightShape = table.Column<int>(type: "int", nullable: true),
                    radiusShape = table.Column<int>(type: "int", nullable: true),
                    floors = table.Column<int>(type: "int", nullable: false),
                    garagsNum = table.Column<int>(type: "int", nullable: false),
                    foundationHeight = table.Column<int>(type: "int", nullable: false),
                    capinaHeight = table.Column<int>(type: "int", nullable: true),
                    liftWidth = table.Column<int>(type: "int", nullable: true),
                    rightWidth = table.Column<int>(type: "int", nullable: true),
                    centerWidth = table.Column<int>(type: "int", nullable: true),
                    directionWidth = table.Column<int>(type: "int", nullable: false),
                    directionHeight = table.Column<int>(type: "int", nullable: false),
                    wellStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    capinaStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    floorHeights = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    garagsHeights = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    doorDirections = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    garagsDirections = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    workRequied = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    doortwoDirections = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    garagstwoDirections = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    twoDirectionFlag = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WallStatusForList = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TechName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNum = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WellImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DirectionImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResizableImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    stopsCount = table.Column<int>(type: "int", nullable: true),
                    load = table.Column<int>(type: "int", nullable: true),
                    machineType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    countryOfOrigin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    speed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    doorOpenSize = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    doorsDecor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    doorFrameDecor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    elevatorCategory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    stainlessType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sideWalls = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    backWall = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ceiling = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lightingColor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    flooring = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    innerButtons = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    callButtons = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    handrail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dvdScreen = table.Column<bool>(type: "bit", nullable: false),
                    doorSafetyKey = table.Column<bool>(type: "bit", nullable: false),
                    cabinPassword = table.Column<bool>(type: "bit", nullable: false),
                    mobileLine = table.Column<bool>(type: "bit", nullable: false),
                    control = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElevatorSpecification", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ElevatorSpecificationImage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ElevatorSpecificationId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElevatorSpecificationImage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ElevatorSpecificationImage_ElevatorSpecification_ElevatorSpecificationId",
                        column: x => x.ElevatorSpecificationId,
                        principalTable: "ElevatorSpecification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ElevatorSpecificationImage_ElevatorSpecificationId",
                table: "ElevatorSpecificationImage",
                column: "ElevatorSpecificationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ElevatorSpecificationImage");

            migrationBuilder.DropTable(
                name: "ElevatorSpecification");
        }
    }
}
