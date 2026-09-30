using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace maria.Migrations
{
    /// <inheritdoc />
    public partial class PeriodicMaintanceReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PeriodicMaintanceCheckingItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Item = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodicMaintanceCheckingItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PeriodicMaintanceReport",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractId = table.Column<int>(type: "int", nullable: true),
                    SerialNumberOfLift = table.Column<int>(type: "int", nullable: true),
                    VisitNumber = table.Column<int>(type: "int", nullable: true),
                    Capacity = table.Column<int>(type: "int", nullable: true),
                    LiftType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LiftModel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VisitType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReasonOfVisit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ReportNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientSignaturePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TechSignaturePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TechName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNum = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ForTheNotWorkingItems = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodicMaintanceReport", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PeriodicMaintanceItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CheckingItemId = table.Column<int>(type: "int", nullable: false),
                    PeriodicMaintanceReportId = table.Column<int>(type: "int", nullable: false),
                    Working = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodicMaintanceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeriodicMaintanceItems_PeriodicMaintanceCheckingItems_CheckingItemId",
                        column: x => x.CheckingItemId,
                        principalTable: "PeriodicMaintanceCheckingItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PeriodicMaintanceItems_PeriodicMaintanceReport_PeriodicMaintanceReportId",
                        column: x => x.PeriodicMaintanceReportId,
                        principalTable: "PeriodicMaintanceReport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PeriodicMaintanceSparePart",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PeriodicMaintanceReportId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: true),
                    IsSupplied = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodicMaintanceSparePart", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeriodicMaintanceSparePart_PeriodicMaintanceReport_PeriodicMaintanceReportId",
                        column: x => x.PeriodicMaintanceReportId,
                        principalTable: "PeriodicMaintanceReport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "PeriodicMaintanceCheckingItems",
                columns: new[] { "Id", "Item", "Type" },
                values: new object[,]
                {
                    { 1, "عمل جرس الانذار & توقف الطوارئ (فوتوسيل - بروكسيمتي سويتش - ليميت سويتش - مفتاح توقف)\nAlarm & emergency stop work (Photocell - Proximity switch - Limit switch - Stop switch)", "First: the cabin - اولا : الكابينة" },
                    { 2, "سهولة حركة الابواب في الفتح والغلق & سرعة الابواب (الاوتوماتيك)\nEase of movement of the doors in opening and closing & the speed of the doors (automatic)", "First: the cabin - اولا : الكابينة" },
                    { 3, "تنظيف المروحة واماكن التهوية في الكابينة\nCleaning the fan and ventilation places in the cabin", "First: the cabin - اولا : الكابينة" },
                    { 4, "عمل مفاتيح التشغيل & عمل الاضاءة بشكل سليم\nThe operation of the operating keys & the operation of the lighting properly", "First: the cabin - اولا : الكابينة" },
                    { 5, "عمل اضاءة الطوارئ ومفاتيح الطوارئ لنزول المصعد من داخل الكابينة\nWork emergency lighting + emergency keys to get the elevator down from inside the cabin", "First: the cabin - اولا : الكابينة" },
                    { 6, "التحقق من مسامير الكابينة", "First: the cabin - اولا : الكابينة" },
                    { 7, "عمل علبة التشغيل اعلى الكابينة\nThe work of the operating box on top of the cabin", "Second: the doors + the top of the cabin - ثانيا : الابواب + أعلى الكابينة" },
                    { 8, "لميت سويتش ترخية الويرات\nReleasing wire rope limit switch", "Second: the doors + the top of the cabin - ثانيا : الابواب + أعلى الكابينة" },
                    { 9, "تنظيف مجاري الابواب السفلية والعلوية (وضع شحم في حالة الحاجة)\nCleaning the ducts of the lower and upper doors (apply grease if needed)", "Second: the doors + the top of the cabin - ثانيا : الابواب + أعلى الكابينة" },
                    { 10, "عمل الاستدعاء في الادوار\nCalling buttons on floors", "Second: the doors + the top of the cabin - ثانيا : الابواب + أعلى الكابينة" },
                    { 11, "عمل البكر والبلى الخاص بالابواب (صلاحية البرشام في باب الاوكورديون)\nBulley and bearing of doors", "Second: the doors + the top of the cabin - ثانيا : الابواب + أعلى الكابينة" },
                    { 12, "عمل لوك الابواب الميكانيكي والكهربي\nMechanical and electrical door lock", "Second: the doors + the top of the cabin - ثانيا : الابواب + أعلى الكابينة" },
                    { 13, "مراجعة خلوصات الكابينة مع ابواب الادوار في كل دور\nReview the cabin clearances with the doors of the floors on each floor", "Second: the doors + the top of the cabin - ثانيا : الابواب + أعلى الكابينة" },
                    { 14, "سلامة مسار خراطيم الهيدروليك & الكابلات & سوست اسفل الكابينة & الكابل المتحرك مع الكابينة\nSafety (hydraulic hoses track & cables & springs under the cab & moving cable with cab)", "Third: the shaft - ثالثا : البئر" },
                    { 15, "سلامة دلائل الحركة الرئيسية للكابينة وتنظيفها وتزييتها وسلامة المزيتة\nSafety of the main movement guides of the cab, its cleaning and lubrication, and the safety of the lubricator", "Third: the shaft - ثالثا : البئر" },
                    { 16, "تزييت الدلائل & تثبيت أفيز البستم & تثبيت المسامير الخاصة بالكوابل\nLubrication of guides & fixing of piston fuses & fixing screws for cables", "Third: the shaft - ثالثا : البئر" },
                    { 17, "الويرات لا يوجد بها الاتي (لمعة - تفتيل - تطبيق)\nWires does not have the following (shine - wicking - drape)", "Third: the shaft - ثالثا : البئر" },
                    { 18, "البكر لا يوجد به تآكل & الرولمان بلى الخاص بالبكر\nWire rope bulley and bolts", "Third: the shaft - ثالثا : البئر" },
                    { 19, "لميت سويتش ترخية الويرات\nAll limit switches", "Third: the shaft - ثالثا : البئر" },
                    { 20, "شرائح المغناطيس ملصقة\nMagnet strips fixing well", "Third: the shaft - ثالثا : البئر" },
                    { 21, "التحقق من تشغيل الكابينة من علبة الصيانة اعلى الكابينة بسرعة منخفضة\nVerify the cab is operating from the maintenance box at the top of the cab at low speed", "Third: the shaft - ثالثا : البئر" },
                    { 22, "عمل لميتات سويتش البراشوت\nLimit switch of the safety device working well", "Third: the shaft - ثالثا : البئر" },
                    { 23, "فحص غرفة الماكينة ، البئر ، اعلى الكابينة ، اسفل الكابينة ، مجاري الابواب\nInspect the machine room, the shaft, the top of the cabin, the bottom of the cabin, the ducts of the doors", "Fourth: general - رابعا : بنود عامة" },
                    { 24, "تجربة الكابينة . التحقق من عدم وجود ضوضاء غير عادية، التحقق من استواء وقفات الادوار\nCabin checking . Check that there are no unusual noises. Checking the alignment of the floor stops", "Fourth: general - رابعا : بنود عامة" },
                    { 25, "ليميت سويتش الماكينة لبداية ونهاية المشوار\nLimit switch the machine for the beginning and end of the travel", "Machine - الماكينة الدرامات" },
                    { 26, "تشحيم الكاتينة والفتيل\nGreasing for all parts , guide rails , shoes", "Machine - الماكينة الدرامات" },
                    { 27, "الوايرات في مسارتها على الدرام\nAl-Wairat is on its way to the bulley", "Machine - الماكينة الدرامات" },
                    { 28, "ربط افيز الواير على الدرام\nConnect the wire rope clips", "Machine - الماكينة الدرامات" },
                    { 29, "الرولمان بلى الخاص بالمجموعة يعمل بحالة جيدة", "Machine - الماكينة الدرامات" },
                    { 30, "تجربة نزول الطوارئ من الموتور والبطاريات\nEmergency test from motor and batteries", "Machine - الماكينة الدرامات" },
                    { 31, "مراجعة مستوى الزيت\nCheck oil level", "The hydraulic machine - الماكينة الهيدروليك" },
                    { 32, "مراجعة عدم وجود تسرب في جميع أنحاء الصمامات والخراطيم والبساتم\nCheck the absence of leaks throughout valves, hoses and pistons", "The hydraulic machine - الماكينة الهيدروليك" },
                    { 33, "مراجعة نزول الكابينة من صمام اليدوي\nCheck the cab descent from the manual valve", "The hydraulic machine - الماكينة الهيدروليك" },
                    { 34, "التحقق من relays & contractors & selectors & Fuses\nChecking relays & contractors & selectors & Fuses", "Control box - الكنترول" },
                    { 35, "عدم وجود كباري مؤقتة على الروزيتة\nThere are no temporary bridges on the rosette", "Control box - الكنترول" },
                    { 36, "تنظيف الكنترول\nControl cleaning", "Control box - الكنترول" },
                    { 37, "التحقق من الفولت و وصلات الموتور\nChecking the voltage and motor connections", "Control box - الكنترول" },
                    { 38, "اختبار الاوفرلود\nOverload test", "Control box - الكنترول" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicMaintanceItems_CheckingItemId",
                table: "PeriodicMaintanceItems",
                column: "CheckingItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicMaintanceItems_PeriodicMaintanceReportId",
                table: "PeriodicMaintanceItems",
                column: "PeriodicMaintanceReportId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicMaintanceSparePart_PeriodicMaintanceReportId",
                table: "PeriodicMaintanceSparePart",
                column: "PeriodicMaintanceReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PeriodicMaintanceItems");

            migrationBuilder.DropTable(
                name: "PeriodicMaintanceSparePart");

            migrationBuilder.DropTable(
                name: "PeriodicMaintanceCheckingItems");

            migrationBuilder.DropTable(
                name: "PeriodicMaintanceReport");
        }
    }
}
