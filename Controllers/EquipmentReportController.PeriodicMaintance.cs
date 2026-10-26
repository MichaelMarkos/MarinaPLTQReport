using maria.Dto;
using maria.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

// تقرير الزيارة الدورية (زيارة صيانة / اختبار وتسليم - periodic maint. / testing)
public partial class EquipmentReportController
{
    private const string PeriodicMaintanceFolder = "PeriodicMaintance";

    [HttpGet("PeriodicMaintanceCheckingItem")]
    public IActionResult PeriodicMaintanceCheckingItems()
    {
        var items = _db.PeriodicMaintanceCheckingItems.OrderBy(x => x.Id).ToList();
        return Ok(items);
    }

    [HttpGet("GetPagedPeriodicMaintanceReports")]
    public async Task<IActionResult> GetPagedPeriodicMaintanceReports(long userId , string? reportNumber , string? companyName ,
        string? techName , string? phoneNum , string? from , string? to , string? search , int page = 1 , int pageSize = 5)
    {
        var query = _db.PeriodicMaintanceReport
            .Include(x => x.PeriodicMaintanceItems)
            .Include(x => x.SpareParts)
            .Include(x => x.Images)
            .AsQueryable();

        // فلتر المستخدم
        if(userId>0)
            query=query.Where(x => x.UserId==userId);

        // بحث موحد: رقم التقرير أو الشركة أو الفني أو الهاتف
        if(!string.IsNullOrWhiteSpace(search))
            query=query.Where(x => x.ReportNumber.Contains(search)||x.CompanyName.Contains(search)||x.ClientName.Contains(search)||
                x.TechName.Contains(search)||x.PhoneNum.Contains(search));

        // فلتر رقم التقرير
        if(!string.IsNullOrWhiteSpace(reportNumber))
            query=query.Where(x => x.ReportNumber.Contains(reportNumber));

        // فلتر اسم الشركة
        if(!string.IsNullOrWhiteSpace(companyName))
            query=query.Where(x => x.CompanyName.Contains(companyName));

        // فلتر اسم الفني
        if(!string.IsNullOrWhiteSpace(techName))
            query=query.Where(x => x.TechName.Contains(techName));

        // فلتر رقم الهاتف
        if(!string.IsNullOrWhiteSpace(phoneNum))
            query=query.Where(x => x.PhoneNum.Contains(phoneNum));

        // فلتر التاريخ
        if(!string.IsNullOrWhiteSpace(from)&&!string.IsNullOrWhiteSpace(to))
            query=query.Where(x =>
                x.Date.Date>=DateTime.Parse(from)&&x.Date.Date<=DateTime.Parse(to));

        query=query.OrderByDescending(x => x.Id);

        int totalCount = await query.CountAsync();
        var reportlist = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize).ToListAsync();

        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        var reports = reportlist
            .Select(x => MapPeriodicMaintanceReport(x , new PeriodicMaintanceReportDto() , baseUrl))
            .ToList();

        return Ok(new
        {
            totalCount ,
            page ,
            pageSize ,
            totalPages = (int)Math.Ceiling(totalCount/(double)pageSize) ,
            reports
        });
    }

    [HttpGet("GetPeriodicMaintanceReportDetails/{id}")]
    public async Task<IActionResult> GetPeriodicMaintanceReportDetails(int id)
    {
        var report = await _db.PeriodicMaintanceReport
            .Include(x => x.PeriodicMaintanceItems)
            .Include(x => x.SpareParts)
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == id);

        if(report==null)
            return NotFound();

        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        var result = MapPeriodicMaintanceReport(report , new PeriodicMaintanceReportDetailDto() , baseUrl);

        if(report.ContractId!=null)
        {
            var projects = await GetMaintenanceProjects((int)report.ContractId);
            var project = projects.FirstOrDefault();
            result.ContractDetails=project!=null
                ? $"ClientName: {project.Client} - ProductSerial: {project.ProductSerial} - StartDate :{project.ContractStartDate} - EndDate :{project.ContractEndDate} "
                : null;
        }

        result.checkingItems=await GetPeriodicMaintanceCheckingItems(report);

        return Ok(result);
    }

    [HttpPost("addPeriodicMaintanceReport")]
    public async Task<IActionResult> addPeriodicMaintanceReport([FromForm] IFormCollection request)
    {
        try
        {
            var itemsJson = request["items"];
            if(string.IsNullOrEmpty(itemsJson))
                return BadRequest("No items data received.");

            var items = DeserializePeriodicMaintanceItems(itemsJson);

            var currentYear = DateTime.Now.Year.ToString().Substring(2);

            // ابحث عن آخر تقرير في نفس السنة
            var lastReport = _db.PeriodicMaintanceReport
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

            var report = new PeriodicMaintanceReport
            {
                ReportNumber = $"{currentYear}/{nextNumber:D3}",
                UserId = long.TryParse(request["userId"], out var userId) ? userId : 1,
            };
            FillPeriodicMaintanceReport(report , request);
            await SavePeriodicMaintanceSignatures(report , request);

            _db.PeriodicMaintanceReport.Add(report);
            await _db.SaveChangesAsync();

            AddPeriodicMaintanceItems(report.Id , items);
            AddPeriodicMaintanceSpareParts(report.Id , request);
            var imagePaths = await SavePeriodicMaintanceImages(report.Id , request);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "✅ تم حفظ البيانات والتواقيع بنجاح" ,
                id = report.Id ,
                reportNumber = report.ReportNumber ,
                imagePaths
            });
        }
        catch(Exception ex)
        {
            return StatusCode(500 , ex.Message);
        }
    }

    [HttpPost("editPeriodicMaintance")]
    public async Task<IActionResult> editPeriodicMaintance([FromForm] IFormCollection request)
    {
        try
        {
            var itemsJson = request["items"];
            if(string.IsNullOrEmpty(itemsJson))
                return BadRequest("No items data received.");

            var items = DeserializePeriodicMaintanceItems(itemsJson);

            var id = int.Parse(request["Id"]);
            var report = _db.PeriodicMaintanceReport.FirstOrDefault(r => r.Id == id);
            if(report==null)
                return NotFound();

            FillPeriodicMaintanceReport(report , request);
            // التواقيع تستبدل فقط لو تم ارسال توقيع جديد
            await SavePeriodicMaintanceSignatures(report , request);

            _db.PeriodicMaintanceReport.Update(report);

            _db.PeriodicMaintanceItems.RemoveRange(_db.PeriodicMaintanceItems.Where(x => x.PeriodicMaintanceReportId==id));
            AddPeriodicMaintanceItems(report.Id , items);

            _db.PeriodicMaintanceSparePart.RemoveRange(_db.PeriodicMaintanceSparePart.Where(x => x.PeriodicMaintanceReportId==id));
            AddPeriodicMaintanceSpareParts(report.Id , request);

            // الصور الجديدة تضاف للصور القديمة
            await SavePeriodicMaintanceImages(report.Id , request);

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "✅ تم حفظ البيانات والتواقيع بنجاح" ,
            });
        }
        catch(Exception ex)
        {
            return StatusCode(500 , ex.Message);
        }
    }

    [HttpPost("deletePeriodicMaintance/{id}")]
    public IActionResult deletePeriodicMaintance(int id)
    {
        var report = _db.PeriodicMaintanceReport.FirstOrDefault(x => x.Id == id);
        if(report==null)
            return NotFound();

        _db.PeriodicMaintanceItems.RemoveRange(_db.PeriodicMaintanceItems.Where(x => x.PeriodicMaintanceReportId==id));
        _db.PeriodicMaintanceSparePart.RemoveRange(_db.PeriodicMaintanceSparePart.Where(x => x.PeriodicMaintanceReportId==id));
        _db.PeriodicMaintanceImage.RemoveRange(_db.PeriodicMaintanceImage.Where(x => x.PeriodicMaintanceReportId==id));
        _db.PeriodicMaintanceReport.Remove(report);
        _db.SaveChanges();

        return Ok(new { message = "تم المسح بنجاح" , id = report.Id });
    }

    [HttpGet("GetPeriodicMaintanceReportPdf")]
    public async Task<IActionResult> GetPeriodicMaintanceReportPdf(int Id)
    {
        QuestPDF.Settings.License=LicenseType.Community;

        var fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "Cairo-Regular.ttf");
        FontManager.RegisterFont(System.IO.File.OpenRead(fontPath));

        var report = await _db.PeriodicMaintanceReport
            .Include(x => x.PeriodicMaintanceItems)
            .Include(x => x.SpareParts)
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == Id);

        if(report==null)
            return NotFound("التقرير غير موجود");

        var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "marina-logo.png");
        var logoBytes = System.IO.File.ReadAllBytes(logoPath);

        var techImage = ReadWwwrootFile(report.TechSignaturePath);
        var clientImage = ReadWwwrootFile(report.ClientSignaturePath);

        var checkingItems = await GetPeriodicMaintanceCheckingItems(report);

        const string font = "Cairo";
        const string headerColor = "#C0504D";
        const string sectionColor = "#F2DCDB";

        // خلية عنوان + قيمة (عربي / انجليزي)
        void InfoCell(IContainer c , string ar , string en , string? value)
        {
            c.Border(1).Padding(2).Row(r =>
            {
                r.RelativeItem().AlignMiddle().AlignCenter().Text(value ?? "").FontFamily(font).FontSize(8).Bold();
                r.ConstantItem(70).AlignMiddle().AlignCenter().Column(cc =>
                {
                    cc.Item().AlignCenter().Text(ar).FontFamily(font).FontSize(7);
                    cc.Item().AlignCenter().Text(en).FontFamily(font).FontSize(7);
                });
            });
        }

        // جدول بنود الفحص لمجموعة من الأقسام
        void ChecksTable(IContainer c , string[] types)
        {
            c.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(45);
                    columns.ConstantColumn(45);
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Border(1).Background(headerColor).Padding(2).AlignCenter().AlignMiddle().Column(cc =>
                    {
                        cc.Item().AlignCenter().Text("لا تعمل").FontFamily(font).FontSize(7).FontColor(Colors.White).Bold();
                        cc.Item().AlignCenter().Text("not working").FontFamily(font).FontSize(7).FontColor(Colors.White);
                    });
                    header.Cell().Border(1).Background(headerColor).Padding(2).AlignCenter().AlignMiddle().Column(cc =>
                    {
                        cc.Item().AlignCenter().Text("تعمل").FontFamily(font).FontSize(7).FontColor(Colors.White).Bold();
                        cc.Item().AlignCenter().Text("working").FontFamily(font).FontSize(7).FontColor(Colors.White);
                    });
                    header.Cell().Border(1).Background(headerColor).Padding(2).AlignCenter().AlignMiddle()
                        .Text("Check items - بنود الفحص").FontFamily(font).FontSize(8).FontColor(Colors.White).Bold();
                });

                foreach(var type in types)
                {
                    // عنوان القسم
                    table.Cell().Border(1);
                    table.Cell().Border(1);
                    table.Cell().Border(1).Background(sectionColor).Padding(3).AlignCenter()
                        .Text(type).FontFamily(font).FontSize(8).Bold();

                    foreach(var item in checkingItems.Where(x => x.Type==type))
                    {
                        table.Cell().Border(1).AlignCenter().AlignMiddle()
                            .Text(item.Working==false ? "✔" : " ").FontFamily(font).FontSize(9);
                        table.Cell().Border(1).AlignCenter().AlignMiddle()
                            .Text(item.Working==true ? "✔" : " ").FontFamily(font).FontSize(9);
                        table.Cell().Border(1).PaddingHorizontal(3).PaddingVertical(1).Column(cc =>
                        {
                            foreach(var line in item.Item.Split('\n'))
                                cc.Item().AlignCenter().Text(line).FontFamily(font).FontSize(7);
                        });
                    }
                }
            });
        }

        void SignatureBox(IContainer c , string title , string? name , byte[] image)
        {
            c.Column(cc =>
            {
                cc.Item().AlignCenter().Text($"{title} : {name}").FontFamily(font).FontSize(8).Bold();
                cc.Item().AlignCenter().Height(60).Element(e =>
                {
                    if(image.Length>0)
                        e.Image(image).FitArea();
                });
            });
        }

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(15);
                page.Size(PageSizes.A4.Landscape());

                // ===== رأس الصفحة =====
                page.Header().Column(col =>
                {
                    col.Item().Border(1).Row(row =>
                    {
                        row.RelativeItem(2).BorderRight(1).Padding(4).AlignMiddle()
                            .Text($"Report# : {report.ReportNumber}").FontFamily(font).FontSize(9).Bold();

                        row.RelativeItem(3).BorderRight(1).Padding(4).AlignMiddle().Column(cc =>
                        {
                            cc.Item().AlignCenter().Text("زيارة صيانة / اختبار وتسليم").FontFamily(font).FontSize(11).Bold();
                            cc.Item().AlignCenter().Text("periodic maint. / testing").FontFamily(font).FontSize(11).Bold();
                        });

                        row.RelativeItem(3).Padding(4).AlignMiddle().Row(r =>
                        {
                            r.ConstantItem(90).AlignMiddle().Image(logoBytes).FitWidth();
                            r.RelativeItem().AlignMiddle().Column(cc =>
                            {
                                cc.Item().AlignCenter().Text("مارينا قطر للمصاعد").FontFamily(font).FontSize(11).Bold();
                                cc.Item().AlignCenter().Text("Marina Elevators Qatar").FontFamily(font).FontSize(11).Bold();
                            });
                        });
                    });

                    // ===== بيانات الزيارة =====
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Cell().Element(c => InfoCell(c , "رقم المصعد" , "serial number" , report.SerialNumberOfLift?.ToString()));
                        table.Cell().Element(c => InfoCell(c , "العنوان" , "Address" , report.Address));
                        table.Cell().Element(c => InfoCell(c , "العميل" , "customer name" , report.ClientName));

                        table.Cell().Element(c => InfoCell(c , "نوع المصعد" , "Lift Type" , report.LiftType));
                        table.Cell().Element(c => InfoCell(c , "التاريخ" , "Date" , report.Date.ToShortDateString()));
                        table.Cell().Element(c => InfoCell(c , "الشركة" , "Company" , report.CompanyName));

                        table.Cell().Element(c => InfoCell(c , "الحمولة" , "capacity" , report.Capacity?.ToString()));
                        table.Cell().Element(c => InfoCell(c , "موديل المصعد" , "Model" , report.LiftModel));
                        table.Cell().Element(c => InfoCell(c , "نوع الزيارة" , "Visit type" , report.VisitType));

                        // رقم الزيارة الدورية (1 - 20)
                        table.Cell().ColumnSpan(2).Border(1).Padding(2).Row(r =>
                        {
                            r.RelativeItem().AlignMiddle().AlignCenter().Row(nums =>
                            {
                                nums.Spacing(2);
                                for(int i = 1; i<=20; i++)
                                {
                                    var selected = report.VisitNumber==i;
                                    nums.ConstantItem(16).Height(16)
                                        .Border(1).BorderColor(selected ? headerColor : Colors.Grey.Darken1)
                                        .Background(selected ? headerColor : Colors.White)
                                        .AlignCenter().AlignMiddle()
                                        .Text(i.ToString()).FontFamily(font).FontSize(7)
                                        .FontColor(selected ? Colors.White : Colors.Black);
                                }
                            });
                            r.ConstantItem(70).AlignMiddle().Column(cc =>
                            {
                                cc.Item().AlignCenter().Text("رقم الزيارة الدورية").FontFamily(font).FontSize(7);
                                cc.Item().AlignCenter().Text("visit number").FontFamily(font).FontSize(7);
                            });
                        });
                        table.Cell().Element(c => InfoCell(c , "سبب الزيارة الطارئة" , "reason of visit" , report.ReasonOfVisit));
                    });
                    col.Item().Height(4);
                });

                // ===== المحتوى =====
                page.Content().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.Spacing(6);

                        // العمود الأيسر : بنود عامة + الماكينة + الهيدروليك + الكنترول + الملاحظات
                        row.RelativeItem().Column(left =>
                        {
                            left.Item().Element(c => ChecksTable(c , PeriodicMaintanceSeed.LeftColumnTypes));

                            left.Item().Border(1).Row(r =>
                            {
                                r.RelativeItem().Padding(3).Text(report.Notes ?? "").FontFamily(font).FontSize(8);
                                r.ConstantItem(70).BorderLeft(1).Padding(2).AlignMiddle().Column(cc =>
                                {
                                    cc.Item().AlignCenter().Text("ملاحظات العميل").FontFamily(font).FontSize(7);
                                    cc.Item().AlignCenter().Text("Customer notes").FontFamily(font).FontSize(7);
                                });
                            });

                            left.Item().Border(1).Row(r =>
                            {
                                r.RelativeItem().Padding(3).Column(cc =>
                                {
                                    cc.Item().AlignCenter().Text("بالنسبة للبنود التي لا تعمل (تمت الصيانة - تحتاج الى زيارة اخرى)").FontFamily(font).FontSize(7);
                                    cc.Item().AlignCenter().Text("For the non-working items (maintained - need another visit)").FontFamily(font).FontSize(7);
                                    cc.Item().PaddingTop(2).Text(report.ForTheNotWorkingItems ?? "").FontFamily(font).FontSize(8);
                                });
                                r.ConstantItem(70).BorderLeft(1).Padding(2).AlignMiddle().Column(cc =>
                                {
                                    cc.Item().AlignCenter().Text("تقرير فريق الصيانة").FontFamily(font).FontSize(7);
                                    cc.Item().AlignCenter().Text("Report").FontFamily(font).FontSize(7);
                                });
                            });

                            // ===== قطع الغيار (مطلوب / تم التوريد) =====
                            left.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(55);
                                    columns.ConstantColumn(55);
                                    columns.ConstantColumn(40);
                                    columns.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    header.Cell().ColumnSpan(4).Border(1).Background(headerColor).Padding(2).AlignCenter()
                                        .Text("Spare parts - قطع الغيار").FontFamily(font).FontSize(8).FontColor(Colors.White).Bold();

                                    header.Cell().Border(1).Background(sectionColor).Padding(2).AlignCenter()
                                        .Text("تم التوريد / Supplied").FontFamily(font).FontSize(7);
                                    header.Cell().Border(1).Background(sectionColor).Padding(2).AlignCenter()
                                        .Text("مطلوب / Required").FontFamily(font).FontSize(7);
                                    header.Cell().Border(1).Background(sectionColor).Padding(2).AlignCenter()
                                        .Text("العدد / Qty").FontFamily(font).FontSize(7);
                                    header.Cell().Border(1).Background(sectionColor).Padding(2).AlignCenter()
                                        .Text("ق.غ / Spare part").FontFamily(font).FontSize(7);
                                });

                                if(!report.SpareParts.Any())
                                {
                                    table.Cell().ColumnSpan(4).Border(1).Padding(2).AlignCenter()
                                        .Text("-").FontFamily(font).FontSize(8);
                                }

                                foreach(var part in report.SpareParts)
                                {
                                    table.Cell().Border(1).AlignCenter().AlignMiddle()
                                        .Text(part.IsSupplied ? "✔" : " ").FontFamily(font).FontSize(9);
                                    table.Cell().Border(1).AlignCenter().AlignMiddle()
                                        .Text(!part.IsSupplied ? "✔" : " ").FontFamily(font).FontSize(9);
                                    table.Cell().Border(1).AlignCenter().AlignMiddle()
                                        .Text(part.Quantity?.ToString() ?? "-").FontFamily(font).FontSize(8);
                                    table.Cell().Border(1).PaddingHorizontal(3).AlignCenter().AlignMiddle()
                                        .Text(part.Name).FontFamily(font).FontSize(8);
                                }
                            });

                            left.Item().PaddingTop(6).Text($"Phone : {report.PhoneNum}").FontFamily(font).FontSize(8);
                        });

                        // العمود الأيمن : الكابينة + الابواب + البئر
                        row.RelativeItem().Element(c => ChecksTable(c , PeriodicMaintanceSeed.RightColumnTypes));
                    });

                    // ===== التواقيع =====
                    col.Item().PaddingTop(8).Row(row =>
                    {
                        row.RelativeItem().Element(c => SignatureBox(c , "Customer - توقيع العميل" , report.ClientName , clientImage));
                        row.RelativeItem().Element(c => SignatureBox(c , "Technician - فريق الصيانة" , report.TechName , techImage));
                    });
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.CurrentPageNumber().FontFamily(font).FontSize(8);
                    t.Span(" / ").FontFamily(font).FontSize(8);
                    t.TotalPages().FontFamily(font).FontSize(8);
                });
            });
        });

        var pdf = document.GeneratePdf();
        return File(pdf , "application/pdf" , "report.pdf");
    }

    // ===================== Helpers =====================

    private static List<PeriodicMaintanceItemDto> DeserializePeriodicMaintanceItems(string itemsJson)
    {
        return System.Text.Json.JsonSerializer.Deserialize<List<PeriodicMaintanceItemDto>>(
            itemsJson,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        ) ?? new List<PeriodicMaintanceItemDto>();
    }

    private static void FillPeriodicMaintanceReport(PeriodicMaintanceReport report , IFormCollection request)
    {
        report.CompanyName=request ["companyName"];
        report.ContractId=int.TryParse(request ["contractId"] , out var contractId) ? contractId : null;
        report.TechName=request ["techName"];
        report.ClientName=request ["clientName"];
        report.Date=DateTime.TryParse(request ["date"] , out var parsedDate) ? parsedDate : DateTime.Now;
        report.PhoneNum=request ["phoneNum"];
        report.Address=request ["address"];
        report.SerialNumberOfLift=int.TryParse(request ["serialNumberOfLift"] , out var serial) ? serial : null;
        report.VisitNumber=int.TryParse(request ["visitNumber"] , out var visitNumber) ? visitNumber : null;
        report.Capacity=int.TryParse(request ["capacity"] , out var capacity) ? capacity : null;
        report.LiftType=request ["liftType"];
        report.LiftModel=request ["liftModel"];
        report.VisitType=request ["visitType"];
        report.ReasonOfVisit=request ["reasonOfVisit"];
        report.Notes=request ["notes"];
        report.ForTheNotWorkingItems=request ["forTheNotWorkingItems"];
    }

    private async Task SavePeriodicMaintanceSignatures(PeriodicMaintanceReport report , IFormCollection request)
    {
        string uploadRoot =
            Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), PeriodicMaintanceFolder);

        if(!Directory.Exists(uploadRoot))
            Directory.CreateDirectory(uploadRoot);

        async Task<string?> Save(string fieldName , string prefix)
        {
            var file = request.Files.FirstOrDefault(f => f.Name == fieldName);
            if(file==null)
                return null;

            string fileName = $"{prefix}_{Guid.NewGuid()}.png";
            using(var stream = new FileStream(Path.Combine(uploadRoot , fileName) , FileMode.Create))
                await file.CopyToAsync(stream);
            return $"/{PeriodicMaintanceFolder}/{fileName}";
        }

        report.ClientSignaturePath=await Save("clientSignature" , "client") ?? report.ClientSignaturePath;
        report.TechSignaturePath=await Save("techSignature" , "tech") ?? report.TechSignaturePath;
    }

    // يتم حفظ البنود التي تم تحديدها فقط (تعمل / لا تعمل)
    private void AddPeriodicMaintanceItems(int reportId , List<PeriodicMaintanceItemDto> items)
    {
        foreach(var item in items.Where(x => x.Working!=null))
        {
            _db.PeriodicMaintanceItems.Add(new PeriodicMaintanceItems
            {
                CheckingItemId = item.CheckingItemId,
                Working = item.Working!.Value,
                PeriodicMaintanceReportId = reportId
            });
        }
    }

    // حفظ الصور المرفوعة (images) في wwwroot/PeriodicMaintance
    private async Task<List<string>> SavePeriodicMaintanceImages(int reportId , IFormCollection request)
    {
        string uploadRoot =
            Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), PeriodicMaintanceFolder);

        if(!Directory.Exists(uploadRoot))
            Directory.CreateDirectory(uploadRoot);

        List<string> imagePaths = new List<string>();
        foreach(var file in request.Files.Where(f => f.Name=="images"))
        {
            string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            using(var stream = new FileStream(Path.Combine(uploadRoot , fileName) , FileMode.Create))
                await file.CopyToAsync(stream);

            var filePath = $"/{PeriodicMaintanceFolder}/{fileName}";
            imagePaths.Add(filePath);
            _db.PeriodicMaintanceImage.Add(new PeriodicMaintanceImage
            {
                PeriodicMaintanceReportId = reportId,
                FileName = fileName,
                FilePath = filePath
            });
        }
        return imagePaths;
    }

    // قطع الغيار اختيارية : spareParts = [{ name, quantity, isSupplied }]
    private void AddPeriodicMaintanceSpareParts(int reportId , IFormCollection request)
    {
        var sparePartsJson = request["spareParts"].ToString();
        if(string.IsNullOrWhiteSpace(sparePartsJson))
            return;

        var spareParts = System.Text.Json.JsonSerializer.Deserialize<List<PeriodicMaintanceSparePartDto>>(
            sparePartsJson,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        ) ?? new List<PeriodicMaintanceSparePartDto>();

        foreach(var part in spareParts.Where(x => !string.IsNullOrWhiteSpace(x.Name)))
        {
            _db.PeriodicMaintanceSparePart.Add(new PeriodicMaintanceSparePart
            {
                PeriodicMaintanceReportId = reportId,
                Name = part.Name,
                Quantity = part.Quantity,
                IsSupplied = part.IsSupplied
            });
        }
    }

    private async Task<List<PeriodicMaintanceCheckingItemDto>> GetPeriodicMaintanceCheckingItems(PeriodicMaintanceReport report)
    {
        var checkItemDb = await _db.PeriodicMaintanceCheckingItems.OrderBy(x => x.Id).ToListAsync();

        return checkItemDb.Select(a =>
        {
            var reportItem = report.PeriodicMaintanceItems.FirstOrDefault(x => x.CheckingItemId == a.Id);
            return new PeriodicMaintanceCheckingItemDto
            {
                CheckingItemId = a.Id,
                Item = a.Item,
                Type = a.Type,
                Working = reportItem?.Working
            };
        }).ToList();
    }

    private static T MapPeriodicMaintanceReport<T>(PeriodicMaintanceReport x , T dto , string baseUrl)
        where T : PeriodicMaintanceReportDto
    {
        dto.Id=x.Id;
        dto.ContractId=x.ContractId;
        dto.ReportNumber=x.ReportNumber;
        dto.Date=x.Date;
        dto.CompanyName=x.CompanyName;
        dto.ClientName=x.ClientName;
        dto.TechName=x.TechName;
        dto.PhoneNum=x.PhoneNum;
        dto.Address=x.Address;
        dto.SerialNumberOfLift=x.SerialNumberOfLift;
        dto.VisitNumber=x.VisitNumber;
        dto.Capacity=x.Capacity;
        dto.LiftType=x.LiftType;
        dto.LiftModel=x.LiftModel;
        dto.VisitType=x.VisitType;
        dto.ReasonOfVisit=x.ReasonOfVisit;
        dto.Notes=x.Notes;
        dto.ForTheNotWorkingItems=x.ForTheNotWorkingItems;
        dto.ClientSignaturePath=x.ClientSignaturePath!=null ? baseUrl+x.ClientSignaturePath : null;
        dto.TechSignaturePath=x.TechSignaturePath!=null ? baseUrl+x.TechSignaturePath : null;
        dto.CheckingItemsCount=x.PeriodicMaintanceItems.Count;
        dto.SpareParts=x.SpareParts.Select(p => new PeriodicMaintanceSparePartDto
        {
            Name = p.Name,
            Quantity = p.Quantity,
            IsSupplied = p.IsSupplied
        }).ToList();
        dto.Images=x.Images.Select(p => baseUrl+p.FilePath).ToList();
        return dto;
    }

    private static byte[] ReadWwwrootFile(string? relativePath)
    {
        if(string.IsNullOrWhiteSpace(relativePath))
            return Array.Empty<byte>();

        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath.TrimStart('/'));
        return System.IO.File.Exists(fullPath) ? System.IO.File.ReadAllBytes(fullPath) : Array.Empty<byte>();
    }
}
