using maria.Dto;
using maria.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

// تقرير مواصفات المصعد: نسخة من تقرير معاينة المصعد (Elevator preview) + المواصفات الفنية والديكورات والطلبات الإضافية
public partial class EquipmentReportController
{
    private const string ElevatorSpecificationFolder = "elevatorspec";

    [HttpPost("PostElevatorSpecificationReport")]
    [RequestSizeLimit(20_000_000)] // 20 MB
    public async Task<IActionResult> PostElevatorSpecificationReport()
    {
        var form = await Request.ReadFormAsync();
        var currentYear = DateTime.Now.Year.ToString().Substring(2);

        // ابحث عن آخر تقرير في نفس السنة
        var lastReport = _db.ElevatorSpecification
            .Where(r => r.ReportNumber.StartsWith(currentYear + "/"))
            .OrderByDescending(r => r.Id)
            .FirstOrDefault();
        int nextNumber = 1;
        if(lastReport!=null)
        {
            var parts = lastReport.ReportNumber.Split('/');
            if(parts.Length==2&&int.TryParse(parts [1] , out int lastNum))
            {
                nextNumber=lastNum+1;
            }
        }

        var report = new ElevatorSpecification
        {
            ReportNumber = $"{currentYear}/{nextNumber:D3}",
            UserId = long.Parse(form["userId"]),
            CreatedAt = DateTime.UtcNow
        };
        FillElevatorSpecification(report , form);

        await SaveElevatorSpecificationFiles(report , form , true);

        _db.ElevatorSpecification.Add(report);
        await _db.SaveChangesAsync();

        return Ok(new { success = true , report.Id , message = "Report saved successfully." });
    }

    [HttpPost("editElevatorSpecificationReport")]
    [RequestSizeLimit(20_000_000)] // 20 MB
    public async Task<IActionResult> editElevatorSpecificationReport()
    {
        var form = await Request.ReadFormAsync();

        var oldReport = await _db.ElevatorSpecification.FirstOrDefaultAsync(x => x.Id == int.Parse(form["reportId"]));
        if(oldReport==null)
            return NotFound("التقرير غير موجود");

        FillElevatorSpecification(oldReport , form);

        // في التعديل يتم تحديث صور الرسومات فقط (مثل تقرير المعاينة)
        await SaveElevatorSpecificationFiles(oldReport , form , false);

        _db.ElevatorSpecification.Update(oldReport);
        await _db.SaveChangesAsync();

        return Ok(new { success = true , oldReport.Id , message = "Report saved successfully." });
    }

    [HttpGet("GetElevatorSpecificationDetails")]
    public async Task<IActionResult> GetElevatorSpecificationDetails(int reportId)
    {
        var query = await _db.ElevatorSpecification.FirstOrDefaultAsync(x => x.Id == reportId);
        if(query==null)
            return NotFound("التقرير غير موجود");

        var project = new ProjectContractDto();
        if(query.ContractId!=null)
        {
            var projects = await GetMaintenanceProjects((int)query.ContractId);
            project=projects.FirstOrDefault();
        }

        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        var dto = _mapper.Map<ElevatorSpecificationDto>(query);
        dto.WellImagePath=baseUrl+dto.WellImagePath;
        dto.DirectionImagePath=baseUrl+dto.DirectionImagePath;
        dto.ResizableImagePath=baseUrl+dto.ResizableImagePath;
        dto.Images=await _db.ElevatorSpecificationImage
            .Where(y => y.ElevatorSpecificationId==reportId)
            .Select(p => baseUrl+p.FilePath)
            .ToListAsync();
        dto.ContractDetails=project!=null ? $"ClientName: {project.Client} - ProductSerial: {project.ProductSerial} - StartDate :{project.ContractStartDate} - EndDate :{project.ContractEndDate} " : null;

        return Ok(new { report = dto });
    }

    [HttpGet("GetPagedElevatorSpecificationReport")]
    public async Task<IActionResult> GetPagedElevatorSpecificationReport(
        long userId ,
        string? reportNumber ,
        string? companyName ,
        string? techName ,
        string? salesName ,
        string? phoneNum ,
        string? from ,
        string? to ,
        string? search ,
        int pageNumber = 1 ,
        int pageSize = 10)
    {
        if(pageNumber<1)
            pageNumber=1;
        if(pageSize<1)
            pageSize=10;

        var query = _db.ElevatorSpecification.AsQueryable();

        // فلتر المستخدم
        if(userId>0)
            query=query.Where(x => x.UserId==userId);

        // بحث موحد: رقم التقرير أو الشركة أو الفني أو الهاتف
        if(!string.IsNullOrWhiteSpace(search))
            query=query.Where(x => x.ReportNumber.Contains(search)||x.CompanyName.Contains(search)||
                x.TechName.Contains(search)||x.PhoneNum.Contains(search));

        // فلتر رقم التقرير
        if(!string.IsNullOrWhiteSpace(reportNumber))
            query=query.Where(x => x.ReportNumber.Contains(reportNumber));

        // فلتر اسم الشركة
        if(!string.IsNullOrWhiteSpace(companyName))
            query=query.Where(x => x.CompanyName.Contains(companyName));

        // فلتر الفني
        if(!string.IsNullOrWhiteSpace(techName))
            query=query.Where(x => x.TechName.Contains(techName));

        // فلتر المبيعات
        if(!string.IsNullOrWhiteSpace(salesName))
            query=query.Where(x => x.salesName.Contains(salesName));

        // فلتر رقم الهاتف
        if(!string.IsNullOrWhiteSpace(phoneNum))
            query=query.Where(x => x.PhoneNum.Contains(phoneNum));

        // فلتر التاريخ
        if(!string.IsNullOrWhiteSpace(from)&&!string.IsNullOrWhiteSpace(to))
            query=query.Where(x =>
                x.CreatedAt.Date>=DateTime.Parse(from)&&x.CreatedAt.Date<=DateTime.Parse(to));

        // ترتيب
        query=query.OrderByDescending(r => r.Id);

        // Total بعد الفلاتر
        var totalCount = await query.CountAsync();

        // Paging
        var reports = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var ids = reports.Select(r => r.Id).ToList();
        var imagesDb = await _db.ElevatorSpecificationImage
            .Where(x => ids.Contains(x.ElevatorSpecificationId))
            .ToListAsync();

        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        var pagedReports = reports.Select(x =>
        {
            var dto = _mapper.Map<ElevatorSpecificationDto>(x);

            dto.floorHeights=CleanElevatorSpecList(x.floorHeights);
            dto.garagsHeights=CleanElevatorSpecList(x.garagsHeights);
            dto.workRequied=CleanElevatorSpecList(x.workRequied);
            dto.doorDirections=CleanElevatorSpecList(x.doorDirections);
            dto.garagsDirections=CleanElevatorSpecList(x.garagsDirections);
            dto.doortwoDirections=CleanElevatorSpecList(x.doortwoDirections);
            dto.garagstwoDirections=CleanElevatorSpecList(x.garagstwoDirections);

            dto.Images=imagesDb
                .Where(y => y.ElevatorSpecificationId==x.Id)
                .Select(p => baseUrl+p.FilePath)
                .ToList();

            dto.imageSva=new [] { x.WellImagePath , x.DirectionImagePath , x.ResizableImagePath }
                .Where(u => !string.IsNullOrEmpty(u))
                .Select(u => baseUrl+u)
                .ToArray();

            return dto;
        }).ToList();

        return Ok(new
        {
            totalCount ,
            pageNumber ,
            pageSize ,
            totalPages = (int)Math.Ceiling(totalCount/(double)pageSize) ,
            reports = pagedReports
        });
    }

    [HttpGet("ElevatorSpecificationDetails/{id}")]
    public async Task<IActionResult> ElevatorSpecificationDetails(int id)
    {
        var report = await _db.ElevatorSpecification.FirstOrDefaultAsync(x => x.Id == id);

        if(report==null)
            return NotFound();

        var result = new SiteReportUpdateDto
        {
            ReportNumber = report.ReportNumber,
            InvoiceNumber = report.InvoiceNumber,
            PhoneNum = report.PhoneNum,
            TechName = report.TechName,
            CompanyName = report.CompanyName,
            Date = report.Date,
        };
        return Ok(result);
    }

    [HttpPost("UpdateElevatorSpecificationReport")]
    public IActionResult UpdateElevatorSpecificationReport([FromBody] SiteReportUpdateDto model)
    {
        var report = _db.ElevatorSpecification.FirstOrDefault(x => x.Id == model.Id);
        if(report==null)
            return NotFound("التقرير غير موجود");

        report.CompanyName=model.CompanyName;
        report.TechName=model.TechName;
        report.PhoneNum=model.PhoneNum;
        report.ReportNumber=model.ReportNumber;
        report.InvoiceNumber=model.InvoiceNumber;

        _db.ElevatorSpecification.Update(report);
        _db.SaveChanges();

        return Ok(new { message = "تم حفظ التقرير بنجاح" , id = report.Id });
    }

    [HttpPost("deleteElevatorSpecification/{id}")]
    public IActionResult deleteElevatorSpecification(int id)
    {
        var report = _db.ElevatorSpecification.FirstOrDefault(x => x.Id == id);
        if(report==null)
            return NotFound("التقرير غير موجود");

        var reportImages = _db.ElevatorSpecificationImage.Where(x => x.ElevatorSpecificationId == id).ToList();
        _db.ElevatorSpecificationImage.RemoveRange(reportImages);
        _db.ElevatorSpecification.Remove(report);
        _db.SaveChanges();

        return Ok(new { message = "تم المسح بنجاح" , id = report.Id });
    }

    [HttpGet("GetElevatorSpecificationReportPdf")]
    public async Task<IActionResult> GetElevatorSpecificationReportPdf(int Id)
    {
        QuestPDF.Settings.License=LicenseType.Community;

        var fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "Cairo-Regular.ttf");
        FontManager.RegisterFont(System.IO.File.OpenRead(fontPath));

        var r = await _db.ElevatorSpecification.FirstOrDefaultAsync(x => x.Id == Id);
        if(r==null)
            return NotFound();

        var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Picture1.jpg");
        var logoBytes = System.IO.File.ReadAllBytes(logoPath);
        var logoBytesFooter = logoBytes;

        var wellImage = ReadElevatorSpecImage(r.WellImagePath);
        var directionImage = ReadElevatorSpecImage(r.DirectionImagePath);
        var resizImage = ReadElevatorSpecImage(r.ResizableImagePath);

        var workRequied = !string.IsNullOrEmpty(r.workRequied)
            ? r.workRequied.Replace("[", "").Replace("]", "").Replace("\"", "").Replace(",", ", ")
            : string.Empty;

        // ألوان مطابقة لشيت مواصفات المصعد
        const string darkHeader = "#44546A";
        const string redHeader = "#C00000";

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(10);
                page.Size(PageSizes.A4);

                // ===== Header =====
                page.Header().ContentFromRightToLeft().Column(col =>
                {
                    // الشعار
                    col.Item().AlignCenter().Element(e => { e.Width(100).Height(50).Image(logoBytes).FitWidth(); });

                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    // عنوان التقرير
                    col.Item().AlignCenter().PaddingTop(5)
                        .Text($" مواصفات مصعد ( {r.reportType} )")
                        .FontFamily("Cairo")
                        .FontSize(16)
                        .Bold();

                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                // ===== Content =====
                page.Content().ContentFromRightToLeft().Column(col =>
                {
                    // Row 1: التاريخ ورقم التقرير
                    col.Item().AlignRight().Row(row =>
                    {
                        row.RelativeItem().Text($"التاريخ: {r.Date:yyyy/MM/dd}").FontFamily("Cairo").FontSize(12);
                        row.RelativeItem().Text($"رقم التقرير: {r.ReportNumber}").FontFamily("Cairo").FontSize(12);
                        row.RelativeItem().Text($"رقم الفاتورة: {r.InvoiceNumber}").FontFamily("Cairo").FontSize(12);
                    });

                    // Row 2: اسم الشركة
                    col.Item().AlignRight().Text($"اسم الشركة أو العميل: {r.CompanyName}").FontFamily("Cairo").FontSize(12);
                    col.Item().AlignRight().Text($"رقم الهاتف: {r.PhoneNum}").FontFamily("Cairo").FontSize(12);

                    // Row 3: مقاسات البئر
                    col.Item().AlignRight().Row(row =>
                    {
                        var parts = new List<string>();
                        if (r.widthShape > 0)
                            parts.Add($"العرض {r.widthShape}");
                        if (r.heightShape > 0)
                            parts.Add($"العمق {r.heightShape}");
                        if (r.radiusShape > 0 && r.shapeType != "circle")
                            parts.Add($"نصف القطر {r.radiusShape}");
                        if (r.radiusShape > 0 && r.shapeType == "circle")
                            parts.Add($" القطر {r.radiusShape}");

                        parts.Add($"         ");

                        if (r.rightWidth > 0)
                            parts.Add($"كتف يمين {r.rightWidth}");
                        if (r.centerWidth > 0)
                            parts.Add($"فتحه الباب {r.centerWidth}");
                        if (r.liftWidth > 0)
                            parts.Add($" كتف شمال  {r.liftWidth}");
                        if (r.WallStatusForList != null)
                            parts.Add($" {r.WallStatusForList}");

                        row.RelativeItem()
                            .Text("مقاسات الغرفة: " + string.Join("   ", parts))
                            .FontFamily("Cairo")
                            .FontSize(10);
                    });

                    // Row 4: مقاسات سيبس
                    col.Item().AlignRight()
                        .Text($"مقاسات سيبس — العرض {r.resizableSquarewidth}    العمق {r.resizableSquareHeight}")
                        .FontFamily("Cairo").FontSize(12);

                    // Row 5: مقاسات فتحة الباب
                    col.Item().AlignRight()
                        .Text($"مقاسات فتحة الباب — العرض {r.directionWidth}   الطول {r.directionHeight}")
                        .FontFamily("Cairo").FontSize(12);

                    // Row 6: ثلاث صور في نفس الصف
                    col.Item().Row(row =>
                    {
                        foreach(var img in new [] { wellImage , directionImage , resizImage })
                        {
                            row.RelativeItem().AlignCenter().Element(e =>
                            {
                                if(img.Length>0)
                                    e.Width(150).Height(150).Image(img);
                            });
                        }
                    });

                    // Row 7: نوع المصعد وعدد الأدوار
                    col.Item().AlignRight().Row(row =>
                    {
                        row.RelativeItem().Text($"نوع المصعد : {r.typeElevator}").FontFamily("Cairo").FontSize(12);
                        row.RelativeItem().Text($"عدد الأدوار : {r.floors}").FontFamily("Cairo").FontSize(12);
                    });

                    // ===== جدول الأدوار =====
                    col.Item().AlignRight().Scale(0.85f).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(50);
                            columns.RelativeColumn(25);
                            columns.RelativeColumn(25);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Border(1).Padding(2).AlignCenter().Text("الأدوار").FontFamily("Cairo").Bold();
                            header.Cell().Border(1).Padding(2).AlignCenter().Text("الارتفاع").FontFamily("Cairo").Bold();
                            header.Cell().Border(1).Padding(2).AlignCenter().Text("الاتجاه").FontFamily("Cairo").Bold();
                        });

                        void Cell(string? text) =>
                            table.Cell().Border(1).Padding(4).AlignCenter().Text(text ?? " ").FontFamily("Cairo").FontSize(9);

                        var heightList = SplitElevatorSpecList(r.floorHeights);
                        var directionList = SplitElevatorSpecList(r.doorDirections);
                        var directionTwoList = SplitElevatorSpecList(r.doortwoDirections);
                        var gargeheightList = SplitElevatorSpecList(r.garagsHeights);
                        var gargedirectionList = SplitElevatorSpecList(r.garagsDirections);
                        var gargeTwodirectionList = SplitElevatorSpecList(r.garagstwoDirections);

                        // صف واحد: الاسم / الارتفاع / الاتجاه (القيم 1001/66 و 1005/70 تعني تحت الانشاء)
                        void FloorRow(string name , List<string> heights , List<string> dirs , List<string> dirs2 , int i ,
                            string heightUnderConstruction , string dirUnderConstruction)
                        {
                            Cell(name);
                            var h = heights.ElementAtOrDefault(i);
                            Cell(h==heightUnderConstruction ? "تحت الانشاء" : h);
                            var d = dirs.ElementAtOrDefault(i);
                            Cell(d==dirUnderConstruction ? "تحت الانشاء" : $"{d}  {dirs2.ElementAtOrDefault(i)}");
                        }

                        // الماكينة
                        Cell("الماكينة");
                        Cell(r.capinaHeight != 0 && r.capinaHeight != null ? r.capinaHeight.ToString() : r.capinaStatus);
                        Cell("  ");

                        // باقي الأدوار (عكسي)
                        for (int i = heightList.Count - 1; i >= 1; i--)
                            FloorRow($"الدور  {i}" , heightList , directionList , directionTwoList , i , "1001" , "66");

                        // الارضي
                        if (heightList.Count > 0)
                            FloorRow("الارضي" , heightList , directionList , directionTwoList , 0 , "1001" , "66");

                        // الجراجات
                        for (int i = 0; i < gargeheightList.Count; i++)
                            FloorRow($" - {i + 1}" , gargeheightList , gargedirectionList , gargeTwodirectionList , i , "1005" , "70");

                        // حفرة البئر
                        Cell("حفره البئر");
                        Cell(r.foundationHeight != 0 ? r.foundationHeight.ToString() : r.wellStatus);
                        Cell("  ");
                    });
                    col.Spacing(5);

                    col.Item().AlignRight().Text($" الاعمال المطلوبة: {workRequied}").FontFamily("Cairo").FontSize(12);

                    // ملاحظات
                    col.Item().AlignRight().Text($"ملاحظات: {r.Notes}").FontFamily("Cairo").FontSize(12);

                    // بيانات الاتصال
                    col.Item().AlignLeft().PaddingLeft(20).Text($"الفني المسئول: {r.TechName}").FontFamily("Cairo").FontSize(12);
                    col.Item().AlignLeft().PaddingLeft(20).Text($"مسئول المبيعات: {r.salesName}").FontFamily("Cairo").FontSize(12);

                    // ===== مواصفات المصعد (صفحة جديدة) =====
                    col.Item().PageBreak();

                    col.Item().Border(1).Padding(5).AlignCenter().Text("مواصفات المصعد")
                        .FontFamily("Cairo").FontSize(18).Bold();

                    // صف: عنوان + قيمة (+ وحدة اختيارية)
                    void SpecTable(IContainer container , string? title , string titleColor , string labelColor ,
                        IEnumerable<(string Label, string? Value)> rows , bool labelFirst = true)
                    {
                        container.Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(4);
                                c.RelativeColumn(6);
                            });

                            if(title!=null)
                                table.Header(h => h.Cell().ColumnSpan(2).Background(titleColor).Border(1).Padding(3)
                                    .AlignCenter().Text(title).FontFamily("Cairo").FontSize(11).FontColor(Colors.White).Bold());

                            foreach(var (label, value) in rows)
                            {
                                var labelCell = (IContainer c) => c.Background(labelColor).Border(1).Padding(4).AlignCenter()
                                    .Text(label).FontFamily("Cairo").FontSize(10).FontColor(Colors.White);
                                var valueCell = (IContainer c) => c.Border(1).Padding(4).AlignCenter()
                                    .Text(string.IsNullOrWhiteSpace(value) ? "-" : value).FontFamily("Cairo").FontSize(10);

                                if(labelFirst)
                                {
                                    labelCell(table.Cell());
                                    valueCell(table.Cell());
                                }
                                else
                                {
                                    valueCell(table.Cell());
                                    labelCell(table.Cell());
                                }
                            }
                        });
                    }

                    string YesNo(bool v) => v ? "نعم" : "لا";

                    col.Item().PaddingTop(5).Row(row =>
                    {
                        // يمين: المواصفات الفنية + الطلبات الاضافية
                        row.RelativeItem(4).Column(c =>
                        {
                            c.Spacing(8);
                            c.Item().Element(e => SpecTable(e , null , darkHeader , darkHeader , new (string, string?)[]
                            {
                                ("عدد الادوار", r.floors.ToString()),
                                ("عدد الوقفات", r.stopsCount?.ToString()),
                                ("الحمولة", r.load != null ? $"{r.load} كجم" : null),
                                ("نوع الماكينة", r.machineType),
                                ("بلد المنشأ", r.countryOfOrigin),
                                ("السرعة", !string.IsNullOrWhiteSpace(r.speed) ? $"{r.speed} م/ث" : null),
                            }));

                            c.Item().Element(e => SpecTable(e , "طلبات اضافية" , redHeader , "#757575" , new (string, string?)[]
                            {
                                ("شاشة DVD", YesNo(r.dvdScreen)),
                                ("مفتاح امان على الابواب", YesNo(r.doorSafetyKey)),
                                ("باسورد داخل الكابين", YesNo(r.cabinPassword)),
                                ("خط جوال", YesNo(r.mobileLine)),
                                ("الكنترول", r.control),
                            }));
                        });

                        row.ConstantItem(10);

                        // يسار: الديكورات
                        row.RelativeItem(6).Element(e => SpecTable(e , "الديكورات" , darkHeader , redHeader , new (string, string?)[]
                        {
                            ("نوع الابواب / اتجاه الحركة", r.doorType),
                            ("مقاس الباب مفتوح", r.doorOpenSize),
                            ("ديكور الابواب", r.doorsDecor),
                            ("ديكور فريم الابواب", r.doorFrameDecor),
                            ("فئة المصعد", r.elevatorCategory),
                            ("نوع الستانليس", r.stainlessType),
                            ("الجدران الجانبية", r.sideWalls),
                            ("الجدار الخلفي", r.backWall),
                            ("السقف", r.ceiling),
                            ("لون الاضاءة", r.lightingColor),
                            ("الارضية", r.flooring),
                            ("الازرار الداخلية", r.innerButtons),
                            ("ازرار الاستدعاء", r.callButtons),
                            ("هاندريل", r.handrail),
                        }));
                    });
                });

                // ===== Footer =====
                page.Footer().Row(row =>
                {
                    row.ConstantItem(150).Image(logoBytesFooter).FitWidth();

                    row.RelativeItem().Column(col =>
                    {
                        col.Spacing(5);

                        col.Item().Row(rw =>
                        {
                            rw.RelativeItem().Background("#B91C1C").Padding(5).Text("qatar@marinaplt.com")
                                .FontColor(Colors.White).FontFamily("Cairo").FontSize(12);
                            rw.RelativeItem().Background("#1E3A8A").Padding(5).Text("www.marinaplt.com")
                                .FontColor(Colors.White).FontFamily("Cairo").FontSize(12);
                        });

                        col.Item().Row(rw =>
                        {
                            rw.RelativeItem().Background("#1E3A8A").Padding(5).Text("Tel.: 44 32 32 46")
                                .FontColor(Colors.White).FontFamily("Cairo").FontSize(12);
                            rw.RelativeItem().Background("#B91C1C").Padding(5).Text("Fax: 44 27 70 76")
                                .FontColor(Colors.White).FontFamily("Cairo").FontSize(12);
                        });
                    });
                });
            });
        });

        var pdf = document.GeneratePdf();
        return File(pdf , "application/pdf" , $"{DateTime.Now}-report.pdf");
    }

    // نفس حقول PostElevatorReport / editElevatorReport + حقول المواصفات
    private void FillElevatorSpecification(ElevatorSpecification report , IFormCollection form)
    {
        static int Int(IFormCollection f , string key) => int.TryParse(f [key] , out var v) ? v : 0;
        static int? NullableInt(IFormCollection f , string key) => int.TryParse(f [key] , out var v) ? v : null;
        static bool Bool(IFormCollection f , string key) => bool.TryParse(f [key] , out var v) && v;

        report.Date=DateTime.TryParse(form ["date"] , out var parsedDate) ? parsedDate : DateTime.UtcNow;
        report.CompanyName=form ["companyName"];
        report.ContractId=NullableInt(form , "contractId");
        // التقرير القديم يستقبل personName في الإضافة و salesName في التعديل
        report.salesName=GetFormValueOrDefault(form , "salesName")??form ["personName"];
        report.ProjectAddress=GetFormValueOrDefault(form , "projectAddress");
        report.widthShape=Int(form , "width");
        report.garagsNum=Int(form , "garagsNum");
        report.heightShape=Int(form , "height");
        report.radiusShape=Int(form , "radius");
        report.floors=Int(form , "floors");
        report.foundationHeight=Int(form , "foundationHeight");
        report.resizableSquarewidth=Int(form , "resizableSquarewidth");
        report.resizableSquareHeight=Int(form , "resizableSquareHeight");

        report.wellStatus=form ["wellStatus"];
        report.capinaStatus=form ["capinaStatus"];
        report.directionWidth=Int(form , "directionWidth");
        report.directionHeight=Int(form , "directionHeight");
        report.liftWidth=Int(form , "liftWidth");
        report.rightWidth=Int(form , "rightWidth");
        report.centerWidth=Int(form , "centerWidth");
        report.capinaHeight=Int(form , "capinaHeight");

        report.floorHeights=form ["floorHeights"];
        report.garagsHeights=form ["garagsHeights"];
        report.shapeType=form ["shapeType"];
        report.typeElevator=form ["typeElevator"];
        report.workRequied=form ["workRequied"];
        report.doorDirections=form ["doorDirections"];
        report.doortwoDirections=form ["doortwoDirections"];
        report.garagsDirections=form ["garagsDirections"];
        report.garagstwoDirections=form ["garagstwoDirections"];
        report.twoDirectionFlag=Bool(form , "twoDirectionFlag");

        report.Notes=form ["notes"];
        report.PhoneNum=form ["phoneNum"];
        report.reportType=form ["reportType"];
        report.TechName=form ["techName"];
        report.WallStatusForList=form ["wallStatusForList"];

        // ===== المواصفات الفنية =====
        report.stopsCount=NullableInt(form , "stopsCount");
        report.load=NullableInt(form , "load");
        report.machineType=GetFormValueOrDefault(form , "machineType");
        report.countryOfOrigin=GetFormValueOrDefault(form , "countryOfOrigin");
        report.speed=GetFormValueOrDefault(form , "speed");

        // ===== الديكورات =====
        report.doorType=GetFormValueOrDefault(form , "doorType");
        report.doorOpenSize=GetFormValueOrDefault(form , "doorOpenSize");
        report.doorsDecor=GetFormValueOrDefault(form , "doorsDecor");
        report.doorFrameDecor=GetFormValueOrDefault(form , "doorFrameDecor");
        report.elevatorCategory=GetFormValueOrDefault(form , "elevatorCategory");
        report.stainlessType=GetFormValueOrDefault(form , "stainlessType");
        report.sideWalls=GetFormValueOrDefault(form , "sideWalls");
        report.backWall=GetFormValueOrDefault(form , "backWall");
        report.ceiling=GetFormValueOrDefault(form , "ceiling");
        report.lightingColor=GetFormValueOrDefault(form , "lightingColor");
        report.flooring=GetFormValueOrDefault(form , "flooring");
        report.innerButtons=GetFormValueOrDefault(form , "innerButtons");
        report.callButtons=GetFormValueOrDefault(form , "callButtons");
        report.handrail=GetFormValueOrDefault(form , "handrail");

        // ===== طلبات اضافية =====
        report.dvdScreen=Bool(form , "dvdScreen");
        report.doorSafetyKey=Bool(form , "doorSafetyKey");
        report.cabinPassword=Bool(form , "cabinPassword");
        report.mobileLine=Bool(form , "mobileLine");
        report.control=GetFormValueOrDefault(form , "control");

        if(report.shapeType=="square")
        {
            report.radiusShape=null;
        }
        else if(report.shapeType=="circle")
        {
            report.heightShape=null;
            report.widthShape=null;
        }
    }

    // حفظ صور الرسومات (wellImage / directionImage / resizableImage) والصور المرفقة (images)
    private async Task SaveElevatorSpecificationFiles(ElevatorSpecification report , IFormCollection form , bool withImages)
    {
        string uploadRoot = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
            ElevatorSpecificationFolder);

        if(!Directory.Exists(uploadRoot))
            Directory.CreateDirectory(uploadRoot);

        foreach(var file in form.Files)
        {
            if(file.Length==0)
                continue;

            // اسم فريد للملف
            string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            string savePath = Path.Combine(uploadRoot, fileName);

            using(var stream = new FileStream(savePath , FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            string relativePath = $"/{ElevatorSpecificationFolder}/{fileName}";

            switch(file.Name)
            {
                case "wellImage":
                    report.WellImagePath=relativePath;
                    break;

                case "directionImage":
                    report.DirectionImagePath=relativePath;
                    break;

                case "resizableImage":
                    report.ResizableImagePath=relativePath;
                    break;

                case "images" when withImages:
                    _db.ElevatorSpecificationImage.Add(new ElevatorSpecificationImage
                    {
                        FilePath=relativePath ,
                        FileName=file.FileName ,
                        ElevatorSpecification=report
                    });
                    break;
            }
        }
    }

    private static byte[] ReadElevatorSpecImage(string? storedPath)
    {
        if(string.IsNullOrWhiteSpace(storedPath))
            return Array.Empty<byte>();

        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", storedPath.TrimStart('/'));
        return System.IO.File.Exists(path) ? System.IO.File.ReadAllBytes(path) : Array.Empty<byte>();
    }

    // ["1","2"] => 1,2
    private static string CleanElevatorSpecList(string? value) =>
        value != null ? value.Trim('"', '[', ']').Replace("\",\"", ",") : string.Empty;

    // ["1","2"] => [1, 2]
    private static List<string> SplitElevatorSpecList(string? value) =>
        (value ?? string.Empty)
            .Replace("[", "").Replace("]", "").Replace("\"", "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
}
