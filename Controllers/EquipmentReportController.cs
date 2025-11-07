
using DocumentFormat.OpenXml.Bibliography;
using maria.Dto;
using maria.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenXmlPowerTools;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using static maria.Dto.DeliveryReportDetailDto;

[Route("api/[controller]")]
[ApiController]
public class EquipmentReportController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EquipmentReportController(AppDbContext db , IWebHostEnvironment env , IHttpClientFactory httpClientFactory , IHttpContextAccessor httpContextAccessor)
    {
        _db=db;
        _env=env;
        _httpClientFactory=httpClientFactory;
        _httpContextAccessor=httpContextAccessor;
    }


    private string BaseURL
    {
        get
        {
            var uri = _httpContextAccessor?.HttpContext?.Request;
            string Host = uri?.Scheme + "://" + uri?.Host.Value.ToString();
            return Host;
        }
    }



    [HttpPost]
    [RequestSizeLimit(20_000_000)] // 20 MB
    public async Task<IActionResult> PostReport()
    {
        var form = await Request.ReadFormAsync();
        var currentYear = DateTime.Now.Year.ToString();

        // ابحث عن آخر تقرير في نفس السنة
        var lastReport = _db.Reports
            .Where(r => r.ReportNumber.StartsWith(currentYear + "/"))
            .OrderByDescending(r => r.ReportNumber)
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
        var newReportNumber = $"{currentYear}/{nextNumber}";

        var report = new Report
        {
            // Basic Info
            Date = DateTime.TryParse(form["date"], out var parsedDate) ? parsedDate : DateTime.UtcNow,
            ReportType = form["reportType"],
            ReportNumber = newReportNumber,            //InvoiceNumber = form["invoiceNumber"],
            CompanyName = form["companyName"],
            ProjectAddress = GetFormValueOrDefault(form, "projectAddress"),
            EquipmentType = form["equipmentType"],
            ModelMarnia = form["modelMarnia"],
            ModelMarniaHireOrSale = form["modelMarniaHireOrSale"],
            Model = GetFormValueOrDefault(form, "Model"),
            SerialNumber = GetFormValueOrDefault(form, "SerialNumber"),
            WarrantyStatus = form["warrantyStatus"],

            // Numeric Fields
            Cradle = int.TryParse(form["cradle"], out var cradleVal) ? cradleVal : 0,
            Meter = int.TryParse(form["meter"], out var meterVal) ? meterVal : 0,
            Unit = form["unit"],

            Installation = int.TryParse(form["installation"], out var installationVal) ? installationVal : null,
            Removing = int.TryParse(form["removing"], out var removingVal) ? removingVal : null,
            Shifting = int.TryParse(form["shifting"], out var shiftingVal) ? shiftingVal : null,
            PeriodicMaintenance = int.TryParse(form["periodicMaintenance"], out var periodicVal) ? periodicVal : null,
            ThirdParty = int.TryParse(form["thirdParty"], out var thirdPartyVal) ? thirdPartyVal : null,
            Breakdown = int.TryParse(form["breakdown"], out var breakdownVal) ? breakdownVal : null,
            Inspection = int.TryParse(form["inspection"], out var inspectionVal) ? inspectionVal : null,
            Delivery = int.TryParse(form["delivery"], out var deliveryVal) ? deliveryVal : null,
            OnScaffolding = int.TryParse(form["onScaffolding"], out var scaffoldingVal) ? scaffoldingVal : null,

            // Text Fields
            spareParts = form["spareParts"],
            Notes = form["notes"],
            PhoneNum = form["phoneNum"],
            // Signatures (paths to be saved after upload)
            ClientSignaturePath = form["clientSignaturePath"],
            TechSignaturePath = form["techSignaturePath"],
            ClientName = form["clientName"],
            TechName = form["techName"],
            UserId = long.Parse(form["userId"]),


            CreatedAt = DateTime.UtcNow
        };


        // حفظ التوقيعات والصور في مجلد
        string uploadRoot = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");

        if(!Directory.Exists(uploadRoot))
            Directory.CreateDirectory(uploadRoot);

        foreach(var file in form.Files)
        {
            if(file.Length==0)
                continue;

            // اسم فريد للملف
            string fileName = $"{Guid.NewGuid()}_{file.FileName}";
            string savePath = Path.Combine(uploadRoot, fileName);

            using(var stream = new FileStream(savePath , FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            string relativePath = $"/uploads/{fileName}";

            switch(file.Name)
            {
                case "clientSignature":
                    report.ClientSignaturePath=relativePath;
                    break;

                case "techSignature":
                    report.TechSignaturePath=relativePath;
                    break;

                case "images":
                    _db.ReportFiles.Add(new ReportImage
                    {
                        FilePath=relativePath ,
                        FileName=file.FileName ,
                        Report=report
                    });
                    break;


                    //case "pdfFile":
                    //    report.PdfFilePath = relativePath;
                    //    break;
            }
        }

        _db.Reports.Add(report);
        await _db.SaveChangesAsync();

        return Ok(new { success = true , report.Id , message = "Report saved successfully." });
    }

    // Endpoint لاسترجاع الملفات (مثلاً الصور)
    [HttpGet("{id}")]
    public async Task<IActionResult> GetReport(int id)
    {
        var report = await _db.Reports
      .Include(r => r.ReportFiles) // ✅ هذا هو الشكل الصحيح
      .FirstOrDefaultAsync(r => r.Id == id);


        if(report==null)
            return NotFound();

        return Ok(report);
    }

    [HttpGet("CheckingItem")]
    public IActionResult CheckingItem()
    {
        var report =  _db.CheckingItems.ToList();


        if(report==null)
            return NotFound();

        return Ok(report);
    }

    [HttpGet("GetAllReports")]
    public async Task<IActionResult> GetAllReports(long userId , int pageNumber = 1 , int pageSize = 10)
    {
        if(pageNumber<1)
            pageNumber=1;
        if(pageSize<1)
            pageSize=10;
       

        var totalReports = await _db.Reports.CountAsync();

        var reports = new List<Report>();

        if(userId>0)
        {
            reports=await _db.Reports
               .Where(x => x.UserId==userId)
               .OrderByDescending(r => r.CreatedAt)
               .Skip((pageNumber-1)*pageSize)
               .Take(pageSize)
               .ToListAsync();
        }
        else
        {
            reports=await _db.Reports
           .OrderByDescending(r => r.CreatedAt)
           .Skip((pageNumber-1)*pageSize)
           .Take(pageSize)
           .ToListAsync();
        }

        var imagesDb = await _db.ReportFiles.ToListAsync();

        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        var pagedReports = reports.Select(x => new GetAllReportDto
        {
            Id = x.Id,
            Date = x.Date,
            ReportNumber = x.ReportNumber,
            ReportType= x.ReportType,
            InvoiceNumber = x.InvoiceNumber,
            CompanyName = x.CompanyName,
            ProjectAddress = x.ProjectAddress,
            EquipmentType = x.EquipmentType,
            ModelMarnia = x.ModelMarnia,
            ModelMarniaHireOrSale = x.ModelMarniaHireOrSale,
            Model = x.Model,
            SerialNumber = x.SerialNumber,
            WarrantyStatus = x.WarrantyStatus,
            specifications = $"{x.Cradle} Cradle  {x.Meter} Meter with ( {x.Unit} ) Suspension Unit ",
            ReasonOfVisitJson = (x.Installation != 0 ? x.Installation + " Installation ," : "") +
                                (x.Removing != 0 ? x.Removing + " Removing ," : "") +
                                (x.Shifting != 0 ? x.Shifting + " Shifting ," : "") +
                                (x.PeriodicMaintenance != 0 ? x.PeriodicMaintenance + " PeriodicMaintenance ," : "") +
                                (x.ThirdParty != 0 ? x.ThirdParty + " ThirdParty ," : "") +
                                (x.Inspection != 0 ? x.Inspection + " Inspection ," : "") +
                                (x.Delivery != 0 ? x.Delivery + " Delivery ," : "") +
                                (x.OnScaffolding != 0 ? x.OnScaffolding + " OnScaffolding ," : "") ,
            spareParts = ConvertSparePartsToString(x.spareParts),
            Notes = x.Notes,
            CreatedAt = x.CreatedAt,
            ClientName = x.ClientName,
            TechName = x.TechName,
            PhoneNum = x.PhoneNum,
            ClientSignaturePath = baseUrl + x.ClientSignaturePath,
            TechSignaturePath = baseUrl + x.TechSignaturePath,
            Images = imagesDb
                .Where(y => y.ReportId == x.Id)
                .Select(p => baseUrl + p.FilePath)
                .ToList()
        }).ToList();

        return Ok(new
        {
            totalCount = totalReports ,
            pageNumber ,
            pageSize ,
            totalPages = (int)Math.Ceiling(totalReports/(double)pageSize) ,
            reports = pagedReports
        });
    }

    [HttpGet("GetPagedSiteReports")]
    public async Task<IActionResult> GetPagedSiteReports(int page = 1 , int pageSize = 5)
    {
        var query = _db.SiteReports
        .Include(x => x.checkingItemReport)
        .OrderByDescending(x => x.Date);

        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        int totalCount = await query.CountAsync();
        var reports = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(x => new SiteReportDto
        {
            Id = x.Id,
            CompanyName = x.CompanyName,
            Date = x.Date,
            ClientSignaturePath = baseUrl + x.ClientSignaturePath,
            TechSignaturePath = baseUrl + x.TechSignaturePath,
            CheckingItemsCount = x.checkingItemReport.Count
        })
        .ToListAsync();

        return Ok(new
        {
            totalCount ,
            page ,
            pageSize ,
            totalPages = (int)Math.Ceiling(totalCount/(double)pageSize) ,
            reports
        });
    }

    [HttpGet("GetSiteReportDetails/{id}")]
    public async Task<IActionResult> GetSiteReportDetails(int id)
    {
        var report = await _db.SiteReports
        .Include(x => x.checkingItemReport)
        .FirstOrDefaultAsync(x => x.Id == id);

        var checkItemDb = await _db.CheckingItems.ToListAsync();

        if(report==null)
            return NotFound();
        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        return Ok(new SiteReportDetailDto
        {
            CompanyName=report.CompanyName ,
            Date=report.Date ,
            ClientSignaturePath=report.ClientSignaturePath!=null ? baseUrl+report.ClientSignaturePath : null ,
            TechSignaturePath=baseUrl+report.TechSignaturePath!=null ? baseUrl+report.TechSignaturePath : null ,
            checkingItems=checkItemDb.Select(a => new CheckingItemsDto
            {
                Item=a.Item ,
                fault=report.checkingItemReport.Where(x => x.CheckingItemId==a.Id).Select(w => w.fault).FirstOrDefault() ,
                CorrectiveAction=report.checkingItemReport.Where(x => x.CheckingItemId==a.Id).Select(w => w.CorrectiveAction).FirstOrDefault() ,
                faultFlag=report.checkingItemReport.Where(x => x.CheckingItemId==a.Id).Select(w => w.faultFlag).FirstOrDefault() ,
                CorrectiveActionFlag=report.checkingItemReport.Where(x => x.CheckingItemId==a.Id).Select(w => w.CorrectiveActionFlag).FirstOrDefault() ,

            }).ToList()
        });


    }


    [HttpGet("GetDeliveryReportDetails/{id}")]
    public async Task<IActionResult> GetDeliveryReportDetails(int id)
    {
        var report = await _db.DeliveryReport
        .Include(x => x.checkingItemReport).ThenInclude(r=>r.deliveryNote)
        .FirstOrDefaultAsync(x => x.Id == id);

       // var deliveryNoteDb = await _db.DeliveryNotes.ToListAsync();

        if(report==null)
            return NotFound();
        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        return Ok(new DeliveryReportDetailDto
        {
            CompanyName=report.CompanyName ,
            ReportNumber=report.ReportNumber ,
            Date=report.Date ,
            ClientSignaturePath=report.ClientSignaturePath!=null ? baseUrl+report.ClientSignaturePath : null ,
            TechSignaturePath=baseUrl+report.TechSignaturePath!=null ? baseUrl+report.TechSignaturePath : null ,
            checkingItems=report.checkingItemReport.Select(a => new DeliveryItemsDto
            {
               Description = a.deliveryNote.Description ,
               DeliveryType=a.deliveryNote.DeliveryType ,
               Quantity= a.Quantity,
               Unit=a.UnitValue != null ? a.UnitValue :null
            }).ToList()
        });


    }




    [HttpGet("DeliveryNote")]
    public IActionResult DeliveryNote(string deliveryType)
    {
        var report =  _db.DeliveryNotes.Where(x=>x.DeliveryType == deliveryType).ToList();


        if(report==null)
            return NotFound();

        return Ok(report);
    }

    [HttpGet("{id}/word")]
    public async Task<IActionResult> GetWordReport(int id)
    {
        var report = await _db.Reports.FirstOrDefaultAsync(r => r.Id == id);
        if(report==null)
            return NotFound();

        string html = $@"
        <html>
        <head><meta charset='utf-8'></head>
        <body>
            <h2>Equipment Site Report</h2>
            <p><strong>Date:</strong> {report.Date:yyyy-MM-dd}</p>
            <p><strong>Company:</strong> {report.CompanyName}</p>
            <p><strong>Project:</strong> {report.ProjectAddress}</p>
            <p><strong>Equipment Type:</strong> {report.EquipmentType}</p>
            <p><strong>Notes:</strong><br>{report.Notes}</p>
            <img src='https://lipsum.app/60x48/' width='300'/>
        </body>
        </html>";

        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(html);
        return File(bytes , "application/msword" , $"EquipmentReport_{id}.doc");
    }

    [HttpPost("CheckingItemReportList")]
    public async Task<IActionResult> CheckingItemReportList([FromForm] IFormCollection request)
    {
        try
        {
            
         



                var itemsJson = request ["items"];
            if(string.IsNullOrEmpty(itemsJson))
                return BadRequest("No items data received.");


            // var Items = System.Text.Json.JsonSerializer.Deserialize<List<CheckingItemDto>>(itemsJson)!;


            var Items = System.Text.Json.JsonSerializer.Deserialize<List<CheckingItemDto>>(
    itemsJson,
    new System.Text.Json.JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    }
)!;

            string uploadRoot = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "UploadSiteReport");

            if(!Directory.Exists(uploadRoot))
                Directory.CreateDirectory(uploadRoot);

            // حفظ الصور

            // ✅ حفظ الصور





            // حفظ التواقيع
            string? clientSignaturePath = null;
            string? techSignaturePath = null;
            var clientSig = request.Files.FirstOrDefault(f => f.Name == "clientSignature");
            var techSig = request.Files.FirstOrDefault(f => f.Name == "techSignature");

            var sitereport = new SiteReport
            {
                CompanyName = request["companyName"],
                ClientName = request["clientName"],
                TechName = request["techName"],
                UserId = long.Parse(request["userId"]),
                Date = DateTime.TryParse(request["date"], out var parsedDate) ? parsedDate : DateTime.Now
            };

            if(clientSig!=null)
            {
                string fileName = $"client_{Guid.NewGuid()}.png";
                string fullPath = Path.Combine(uploadRoot, fileName);
                using(var stream = new FileStream(fullPath , FileMode.Create))
                    await clientSig.CopyToAsync(stream);
                clientSignaturePath=$"/UploadSiteReport/{fileName}";
                sitereport.ClientSignaturePath=clientSignaturePath;
            }

            if(techSig!=null)
            {
                string fileName = $"tech_{Guid.NewGuid()}.png";
                string fullPath = Path.Combine(uploadRoot, fileName);
                using(var stream = new FileStream(fullPath , FileMode.Create))
                    await techSig.CopyToAsync(stream);
                techSignaturePath=$"/UploadSiteReport/{fileName}";
                sitereport.TechSignaturePath=techSignaturePath;
            }


            _db.SiteReports.Add(sitereport);
            await _db.SaveChangesAsync();

            List<string> imagePaths = new List<string>();
            foreach(var file in request.Files.Where(f => f.Name=="images"))
            {
                string fileName = $"{Guid.NewGuid()}_{file.FileName}";
                string fullPath = Path.Combine(uploadRoot, fileName);
                using(var stream = new FileStream(fullPath , FileMode.Create))
                    await file.CopyToAsync(stream);
                imagePaths.Add($"/UploadSiteReport/{fileName}");
                _db.SiteReportImages.Add(new SiteReportImage
                {
                    siteReportId=sitereport.Id ,
                    FileName=fileName ,
                    FilePath=fullPath

                });
                await _db.SaveChangesAsync();

            }




            // تحويل العناصر القادمة من JSON إلى كائنات
            foreach(var item in Items)
            {
                var report = new CheckingItemReport
                {
                    CheckingItemId = item.CheckingItemId,
                    fault = item.Fault,
                    CorrectiveAction = item.CorrectiveAction,
                    faultFlag = item.faultFlag,
                    CorrectiveActionFlag = item.CorrectiveActionFlag,
                    //CreatedAt = DateTime.Now,
                    SiteReportId = sitereport.Id
                };
                if(string.IsNullOrEmpty(item.Fault)&&string.IsNullOrEmpty(item.CorrectiveAction)&&item.faultFlag==false&&item.CorrectiveActionFlag==item.faultFlag==false)
                    continue;

                _db.CheckingItemReports.Add(report);
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "✅ تم حفظ البيانات والتواقيع بنجاح" ,
                imagePaths
            });
        }
        catch(Exception ex)
        {
            return StatusCode(500 , ex.Message);
        }
    }

    [HttpPost("DeliveryNoteReportList")]
    public async Task<IActionResult> DeliveryNoteReportList([FromForm] IFormCollection request)
    {
        try
        {
            var currentYear = DateTime.Now.Year.ToString();

            var lastReport = _db.Reports
            .Where(r => r.ReportNumber.StartsWith(currentYear + "/"))
            .OrderByDescending(r => r.ReportNumber)
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
            var newReportNumber = $"{currentYear}/{nextNumber}";




            var itemsJson = request["items"];
            if(string.IsNullOrEmpty(itemsJson))
                return BadRequest("No items data received.");


            // var Items = System.Text.Json.JsonSerializer.Deserialize<List<CheckingItemDto>>(itemsJson)!;


            var Items = System.Text.Json.JsonSerializer.Deserialize<List<DeliveryNoteDto>>(
    itemsJson,
    new System.Text.Json.JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    }
)!;




            var itemsJson1 = request["items1"];
            if(string.IsNullOrEmpty(itemsJson1))
                return BadRequest("No items data received.");
            var Items1 = System.Text.Json.JsonSerializer.Deserialize<List<DeliveryNoteDto>>( itemsJson1, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
)!;

            var itemsJson2 = request["items2"];
            if(string.IsNullOrEmpty(itemsJson2))
                return BadRequest("No items data received.");
            var Items2 = System.Text.Json.JsonSerializer.Deserialize<List<DeliveryNoteDto>>( itemsJson2, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
)!;

            var itemsJson3 = request["items3"];
            if(string.IsNullOrEmpty(itemsJson3))
                return BadRequest("No items data received.");
            var Items3 = System.Text.Json.JsonSerializer.Deserialize<List<DeliveryNoteDto>>( itemsJson3, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
)!;

            var itemsJson4 = request["items4"];
            if(string.IsNullOrEmpty(itemsJson4))
                return BadRequest("No items data received.");
            var Items4 = System.Text.Json.JsonSerializer.Deserialize<List<DeliveryNoteDto>>( itemsJson4, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
)!;


            var itemsJson5 = request["items5"];
            if(string.IsNullOrEmpty(itemsJson5))
                return BadRequest("No items data received.");
            var Items5 = System.Text.Json.JsonSerializer.Deserialize<List<DeliveryNoteDto>>( itemsJson5, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
)!;

            string uploadRoot = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "UploadSiteReport");

            if(!Directory.Exists(uploadRoot))
                Directory.CreateDirectory(uploadRoot);

            // حفظ الصور

            // ✅ حفظ الصور





            // حفظ التواقيع
            string? clientSignaturePath = null;
            string? techSignaturePath = null;
            var clientSig = request.Files.FirstOrDefault(f => f.Name == "clientSignature");
            var techSig = request.Files.FirstOrDefault(f => f.Name == "techSignature");

            var deliveryreport = new DeliveryReport
            {
                CompanyName = request["companyName"],
                ClientName = request["clientName"],
                TechName = request["techName"],
                Date = DateTime.TryParse(request["date"], out var parsedDate) ? parsedDate : DateTime.Now,
                PhoneNum = request["phoneNum"],
                Notes = request["notes"],
                ReportNumber = newReportNumber,
                UserId = long.Parse(request["userId"]),
            };

            if(clientSig!=null)
            {
                string fileName = $"client_{Guid.NewGuid()}.png";
                string fullPath = Path.Combine(uploadRoot, fileName);
                using(var stream = new FileStream(fullPath , FileMode.Create))
                    await clientSig.CopyToAsync(stream);
                clientSignaturePath=$"/UploadSiteReport/{fileName}";
                deliveryreport.ClientSignaturePath=clientSignaturePath;
            }

            if(techSig!=null)
            {
                string fileName = $"tech_{Guid.NewGuid()}.png";
                string fullPath = Path.Combine(uploadRoot, fileName);
                using(var stream = new FileStream(fullPath , FileMode.Create))
                    await techSig.CopyToAsync(stream);
                techSignaturePath=$"/UploadSiteReport/{fileName}";
                deliveryreport.TechSignaturePath=techSignaturePath;
            }


            _db.DeliveryReport.Add(deliveryreport);
            await _db.SaveChangesAsync();

            List<string> imagePaths = new List<string>();
            foreach(var file in request.Files.Where(f => f.Name=="images"))
            {
                string fileName = $"{Guid.NewGuid()}_{file.FileName}";
                string fullPath = Path.Combine(uploadRoot, fileName);
                using(var stream = new FileStream(fullPath , FileMode.Create))
                    await file.CopyToAsync(stream);
                imagePaths.Add($"/UploadSiteReport/{fileName}");
                _db.DelivryReportImages.Add(new DelivryReportImage
                {
                    deliveryReportId=deliveryreport.Id ,
                    FileName=fileName ,
                    FilePath=fullPath

                });
                await _db.SaveChangesAsync();

            }




            foreach(var item in Items)
            {
                if(item.quantity=="0"||item.checkingItemId==0)
                    continue;

                var report = new DeliveryNoteReport
                {
                    deliveryNoteId = item.checkingItemId,
                    Quantity = int.Parse( item.quantity),
                    deliveryReportId = deliveryreport.Id,

                };


                _db.DeliveryNoteReport.Add(report);
            }

            foreach(var item in Items1)
            {
                if(item.quantity=="0"||item.checkingItemId==0)
                    continue;

                var report = new DeliveryNoteReport
                {
                    deliveryNoteId = item.checkingItemId,
                    Quantity = int.Parse( item.quantity),
                    deliveryReportId = deliveryreport.Id,

                };


                _db.DeliveryNoteReport.Add(report);
            }
            foreach(var item in Items2)
            {
                if(item.quantity=="0"||item.checkingItemId==0)
                    continue;

                var report = new DeliveryNoteReport
                {
                    deliveryNoteId = item.checkingItemId,
                    Quantity = int.Parse( item.quantity),
                    deliveryReportId = deliveryreport.Id,

                };


                _db.DeliveryNoteReport.Add(report);
            }
            foreach(var item in Items3)
            {
                if(item.quantity=="0"||item.checkingItemId==0)
                    continue;

                var report = new DeliveryNoteReport
                {
                    deliveryNoteId =item.checkingItemId,
                    Quantity = int.Parse( item.quantity),
                    deliveryReportId = deliveryreport.Id,

                };


                _db.DeliveryNoteReport.Add(report);
            }
            foreach(var item in Items4)
            {
                if(item.quantity=="0"||item.checkingItemId==0)
                    continue;

                var report = new DeliveryNoteReport
                {
                    deliveryNoteId = item.checkingItemId,
                    Quantity = int.Parse( item.quantity),
                    deliveryReportId = deliveryreport.Id,
                    UnitValue = !string.IsNullOrEmpty(item.unit) ? int.Parse(item.unit) : null
                };


                _db.DeliveryNoteReport.Add(report);
            }
            foreach(var item in Items5)
            {
                if(item.quantity=="0"||item.checkingItemId==0)
                    continue;

                var report = new DeliveryNoteReport
                {
                    deliveryNoteId = item.checkingItemId,
                    Quantity = int.Parse( item.quantity),
                    deliveryReportId = deliveryreport.Id,
                    UnitValue = !string.IsNullOrEmpty(item.unit) ? int.Parse(item.unit) : null

                };


                _db.DeliveryNoteReport.Add(report);
            }


            var scissorliftsJson= request["scissorliftsList"];

            var scissorlifts = System.Text.Json.JsonSerializer.Deserialize<List<scissorliftsDto>>( scissorliftsJson, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if(scissorlifts.Count>0)
            {
                foreach(var item in scissorlifts)
                {
                    var newDeliveryNote =  _db.DeliveryNotes.Add(new DeliveryNote
                    {
                        Description=item.model+" / "+item.heightModel ,
                        DeliveryType="Scissor lifts",
                        OptionalFlag = true
                    });
                    await _db.SaveChangesAsync();

                    var report = _db.DeliveryNoteReport.Add( new DeliveryNoteReport
                    {
                        deliveryNoteId = newDeliveryNote.Entity.Id ,
                        Quantity = int.Parse( item.quantity),
                        deliveryReportId = deliveryreport.Id,

                    });

                }
            }



            var manliftListJson= request["manliftList"];

            var manliftList = System.Text.Json.JsonSerializer.Deserialize<List<scissorliftsDto>>( manliftListJson, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if(manliftList.Count>0)
            {
                foreach(var item in manliftList)
                {
                    var newDeliveryNote =  _db.DeliveryNotes.Add(new DeliveryNote
                    {
                        Description=item.model+" / "+item.heightModel ,
                        DeliveryType="Man lifts",
                        OptionalFlag = true

                    });
                    await _db.SaveChangesAsync();

                    var report =_db.DeliveryNoteReport.Add( new DeliveryNoteReport
                    {
                        deliveryNoteId = newDeliveryNote.Entity.Id ,
                        Quantity = int.Parse( item.quantity),
                        deliveryReportId = deliveryreport.Id,

                    });

                }
            }





            var productListJson= request["productList"];

            var productList = System.Text.Json.JsonSerializer.Deserialize<List<productListDto>>( productListJson, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if(productList.Count>0)
            {
                foreach(var item in productList)
                {
                    var newDeliveryNote =  _db.DeliveryNotes.Add(new DeliveryNote
                    {
                        Description=item.description ,
                        DeliveryType="Other Products",
                        OptionalFlag = true

                    });
                    await _db.SaveChangesAsync();

                    var report = _db.DeliveryNoteReport.Add( new DeliveryNoteReport
                    {
                        deliveryNoteId = newDeliveryNote.Entity.Id ,
                        Quantity = int.Parse( item.quantity),
                        deliveryReportId = deliveryreport.Id,

                    });

                }
            }





            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "✅ تم حفظ البيانات والتواقيع بنجاح" ,
                imagePaths
            });
        }
        catch(Exception ex)
        {
            return StatusCode(500 , ex.Message);
        }
    }


    [HttpGet("GetPagedDeliveryReports")]
    public async Task<IActionResult> GetPagedDeliveryReports(int page = 1 , int pageSize = 5)
    {
        var query = _db.DeliveryReport
        .Include(x => x.checkingItemReport)
        .OrderByDescending(x => x.Date);

        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        int totalCount = await query.CountAsync();
        var reports = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(x => new DeliveryReportListDto
        {
            Id = x.Id,
            CompanyName = x.CompanyName,
            Date = x.Date,
            ClientSignaturePath = baseUrl + x.ClientSignaturePath,
            TechSignaturePath = baseUrl + x.TechSignaturePath,
            DelveryNoteCount = x.checkingItemReport.Count,
            PhoneNum = x.PhoneNum,
            Notes = x.Notes
        })
        .ToListAsync();

        return Ok(new
        {
            totalCount ,
            page ,
            pageSize ,
            totalPages = (int)Math.Ceiling(totalCount/(double)pageSize) ,
            reports
        });
    }
    [HttpGet("pdf")]
    public IActionResult GetReportPdf11()
    {
        QuestPDF.Settings.License=LicenseType.Community;

        var fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "Cairo-Regular.ttf");
        FontManager.RegisterFont(System.IO.File.OpenRead(fontPath));

        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo.png");
        var imageBytes = System.IO.File.ReadAllBytes(imagePath);

        var document = Document.Create(container =>
        {// ضع المسار الكامل إلى صورة الشعار هنا
            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo.png");

            // قراءة الصورة وتحويلها إلى بايت
            byte[] logoBytes = System.IO.File.ReadAllBytes(logoPath);

            container.Page(page =>
            {
                page.Margin(40);

                // ===== رأس الصفحة =====
                page.Header()
       .Row(row =>
       {
           // العمود الأول: الشعار
           row.RelativeColumn(1).Column(col =>
           {
               col.Item().AlignLeft().PaddingBottom(5).Width(60).Height(40)
                   .Image(logoBytes)
                   .FitHeight(); // <-- هذا يضبط الصورة لتناسب المساحة بدون كسر القيود
           });

           // العمود الثاني: العنوان
           row.RelativeColumn(3).AlignRight().Text("تقرير الأداء")
               .FontSize(18)
               .Bold();
       });


                // ===== المحتوى =====
                page.Content()
                    .Column(col =>
                    {
                        col.Spacing(15);
                        col.Item().Text("الاسم: هاني عديب").FontFamily("Cairo");
                        col.Item().Text("النتيجة: 95").FontFamily("Cairo");

                        // ✅ صورة داخل المحتوى مع تنسيق
                        col.Item().AlignCenter().Element(e =>
                        {
                            e.Border(1)
                             .BorderColor(Colors.Grey.Darken2)
                             .Height(150)
                             .Image(imageBytes);
                            //.FitWidth();
                        });
                    });

                // ===== ذيل الصفحة =====
                page.Footer()
                    .AlignCenter()
                    .Text("© 2025 شركتنا")
                    .FontFamily("Cairo")
                    .FontSize(10);
            });
        });

        var pdf = document.GeneratePdf();
        return File(pdf , "application/pdf" , "report.pdf");
    }


    [HttpGet("pdf22")]
    public IActionResult GetReportPdf22()
    {
        QuestPDF.Settings.License=LicenseType.Community;

        var fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "Cairo-Regular.ttf");
        FontManager.RegisterFont(System.IO.File.OpenRead(fontPath));

        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "marina-logo.png");
        var imageBytes = System.IO.File.ReadAllBytes(imagePath);

        var document = Document.Create(container =>
        {// ضع المسار الكامل إلى صورة الشعار هنا
            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "marina-logo.png");

            // قراءة الصورة وتحويلها إلى بايت
            byte[] logoBytes = System.IO.File.ReadAllBytes(logoPath);

            container.Page(page =>
            {
                page.Margin(40);

                // ===== رأس الصفحة =====
                page.Header()
       .Row(row =>
       {
           // العمود الأول: الشعار
           row.RelativeColumn(1).Column(col =>
           {
               //col.Item().AlignLeft().PaddingBottom(5).Width(60).Height(40)
               //    .Image(logoBytes)
               //    //.FitHeight()
               //    .FitWidth(); // <-- هذا يضبط الصورة لتناسب المساحة بدون كسر القيود
               col.Item().AlignLeft().PaddingBottom(5)
                 .Image(logoBytes)
                 //.FitHeight()
                 .FitWidth();
           });

           // العمود الثاني: العنوان
           row.RelativeColumn(3).AlignRight().Text("تقرير الأداء")
               .FontSize(44)
               .Bold();
       });


                // ===== المحتوى =====
                page.Content()
                    .Column(col =>
                    {
                        col.Spacing(15);
                        col.Item().Text("الاسم: هاني عديب").FontFamily("Cairo");
                        col.Item().Text("النتيجة: 95").FontFamily("Cairo");

                        // ✅ صورة داخل المحتوى مع تنسيق
                        col.Item().AlignCenter().Element(e =>
                        {
                            e.Border(1)
                             .BorderColor(Colors.Grey.Darken2)
                             //.Height(150)
                             .Image(imageBytes);
                            //.FitWidth();
                        });
                    });

                // ===== ذيل الصفحة =====
                page.Footer()
                    .AlignCenter()
                    .Text("© 2025 شركتنا")
                    .FontFamily("Cairo")
                    .FontSize(10);
            });
        });

        var pdf = document.GeneratePdf();
        return File(pdf , "application/pdf" , "report.pdf");
    }
    [HttpGet("pdf23")]
    public IActionResult GetReportPdf23(int Id , string InvoiceNum)
    {
        QuestPDF.Settings.License=LicenseType.Community;

        var fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "Cairo-Regular.ttf");
        FontManager.RegisterFont(System.IO.File.OpenRead(fontPath));

        var reportDb = _db.Reports.Where(x=>x.Id == Id).FirstOrDefault();


        var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "marina-logo.png");
        var logoBytes = System.IO.File.ReadAllBytes(logoPath);

        var logoPathFooter = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Picture1.jpg");
        var logoBytesFooter = System.IO.File.ReadAllBytes(logoPathFooter);

        var techPath = reportDb.TechSignaturePath?.TrimStart('/');
        var techImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", techPath);

        byte[] techImage = Array.Empty<byte>();
        if(System.IO.File.Exists(techImagePath))
        {
            techImage=System.IO.File.ReadAllBytes(techImagePath);
        }

        var clientPath = reportDb.ClientSignaturePath?.TrimStart('/');
        var clientImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", clientPath);

        byte[] clientImage = Array.Empty<byte>();
        if(System.IO.File.Exists(clientImagePath))
        {
            clientImage=System.IO.File.ReadAllBytes(clientImagePath);
        }


        var  ReasonOfVisitJson=(reportDb.Installation!=0 ? reportDb.Installation+" Installation ," : "")+
                               (reportDb.Removing!=0 ? reportDb.Removing+" Removing ," : "")+
                               (reportDb.Shifting!=0 ? reportDb.Shifting+" Shifting ," : "")+
                               (reportDb.PeriodicMaintenance!=0 ? reportDb.PeriodicMaintenance+" PeriodicMaintenance ," : "")+
                               (reportDb.ThirdParty!=0 ? reportDb.ThirdParty+" ThirdParty ," : "")+
                               (reportDb.Inspection!=0 ? reportDb.Inspection+" Inspection ," : "")+
                               (reportDb.Delivery!=0 ? reportDb.Delivery+" Delivery ," : "")+
                               (reportDb.OnScaffolding!=0 ? reportDb.OnScaffolding+" OnScaffolding ," : "") ;
        var spareParts =ConvertSparePartsToString(reportDb.spareParts);



        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Size(PageSizes.A4);

                // ===== رأس الصفحة =====
                page.Header()
                    .Column(col =>
                    {
                        // 🖼️ الصورة بعرض الصفحة
                        col.Item()
                            .AlignCenter()
                            .Element(e =>
                            {
                                e.Image(logoBytes)
                                 .FitWidth();  // يجعل الصورة تمتد بعرض الصفحة تلقائيًا
                            });
                        col.Item().LineHorizontal(1)
    .LineColor(Colors.Grey.Lighten2);
                        // 📝 العنوان أسفل الصورة
                        col.Item()
                            .AlignCenter()
                            .PaddingTop(5)
                            .Text($"{reportDb.ReportType}")
                            .FontFamily("Cairo")
                            .FontSize(20)
                            .Bold();
                        col.Item().LineHorizontal(1)
    .LineColor(Colors.Grey.Lighten2);
                    });

                // ===== المحتوى =====
                page.Content()
                    .Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين العناصر
                            row.RelativeItem().Text($"Date : {reportDb.Date.ToShortDateString()}").FontFamily("Cairo").FontSize(12);
                            row.Spacing(60);
                            row.RelativeItem().Text($"Report # : {reportDb.ReportNumber}").FontFamily("Cairo").FontSize(12);
                            row.RelativeItem().Text($"Invoice # : {InvoiceNum}").FontFamily("Cairo").FontSize(12);
                        });

                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين العناصر
                            row.RelativeItem().Text($"Company Name : {reportDb.CompanyName}").FontFamily("Cairo").FontSize(12);
                        });
                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين العناصر
                            row.RelativeItem().Text($"Adress / Project : {reportDb.ProjectAddress}").FontFamily("Cairo").FontSize(12);
                        });

                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين العناصر
                            row.RelativeItem().Text($"Equipment : {reportDb.EquipmentType}").FontFamily("Cairo").FontSize(12);

                        });

                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين العناصر
                            row.RelativeItem().Text($"Specifications : {reportDb.Cradle} Cradle , {reportDb.Meter} Meter , With {reportDb.Unit} suspension Unit").FontFamily("Cairo").FontSize(14);

                        });

                        col.Item().LineHorizontal(1)
    .LineColor(Colors.Grey.Lighten2);

                        col.Item().Row(row =>
                        {
                            row.Spacing(15); // المسافة بين العناصر
                            row.RelativeItem().Text($"Model : {reportDb.ModelMarnia}    {reportDb.ModelMarniaHireOrSale}").FontFamily("Cairo").FontSize(12);

                        });


                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين العناصر
                            row.RelativeItem().Text($"Model : {reportDb.Model}                                S.N:{reportDb.SerialNumber}").FontFamily("Cairo").FontSize(14);

                        });


                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين العناصر
                            row.RelativeItem().Text($"Warranty : {reportDb.WarrantyStatus} ").FontFamily("Cairo").FontSize(12);

                        });

                        col.Item().LineHorizontal(1)
    .LineColor(Colors.Grey.Lighten2);

                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين العناصر
                            row.RelativeItem().Text($"Reason Of Visit : {ReasonOfVisitJson} ").FontFamily("Cairo").FontSize(12);

                        });
                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين العناصر
                            row.RelativeItem().Text($"Spare Parts : {spareParts} ").FontFamily("Cairo").FontSize(12);

                        });
                        col.Item().LineHorizontal(1)
    .LineColor(Colors.Grey.Lighten2);

                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين العناصر
                            row.RelativeItem().Text($"Report : {reportDb.Notes} ").FontFamily("Cairo").FontSize(12);

                        });
                        col.Item().LineHorizontal(1)
    .LineColor(Colors.Grey.Lighten2);
                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين العناصر
                            row.RelativeItem().Text($" ").FontFamily("Cairo").FontSize(12);

                        });
                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين العناصر
                            row.RelativeItem().Text($"All Workings have been reviewed at the site and there is no damaged happened due the installation, UN installation and maintenance and the machine is working in a good condition. Please check before leaving.\r\nتمت مراجعه كافه الاعمال بالموقع ولا توجد أى خسائر ناتجة عن أعمال الفك أو التركيب أو الصيانة التى تمت بالموقع والمعدة تعمل بحاله جيدة وشركة مارينا غير مسئوله عن اى ضرر يتم أكتشافة بعد مغادرة الموقع لذلك يرجى مراجعة مكان التركيب جيدا قبل مغادرة الفنيين\r\n ").FontFamily("Cairo").FontSize(12);

                        });
                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين العناصر
                            row.RelativeItem().Text($"PhoneNum. : {reportDb.PhoneNum} ").FontFamily("Cairo").FontSize(12);

                        });

                        col.Item().Row(row =>
                        {
                            row.Spacing(50); // المسافة بين العناصر
                            row.RelativeItem().Text($"Marina REP. : {reportDb.TechName} ").FontFamily("Cairo").FontSize(12);
                            //row.RelativeItem().Text($"PhoneNum. : {reportDb.PhoneNum} ").FontFamily("Cairo").FontSize(12);
                            row.RelativeItem().Text($"Site REP. : {reportDb.ClientName} ").FontFamily("Cairo").FontSize(12);

                        });
                        // صورة داخل المحتوى كمثال إضافي
                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين الصورتين

                            // الصورة الأولى
                            row.RelativeItem().Element(e =>
                            {
                                e.Border(1)
         .BorderColor(Colors.Grey.Darken2)
         .Padding(5)
         .Width(150)
         .Height(100)
         .Image(techImage )
         .FitWidth();
                            });

                            // الصورة الثانية
                            row.RelativeItem().Element(e =>
                            {
                                e.Border(1)
         .BorderColor(Colors.Grey.Darken2)
         .Padding(5)
         .Width(150)
                  .Height(100)
         .Image(clientImage) // استخدم صورة أخرى أو نفس الصورة
         .FitWidth();
                            });
                        });



                    });


                // ===== ذيل الصفحة =====
                page.Footer()
                    .BorderBottom(1)
    .PaddingVertical(10)
    .Row(row =>
    {
        row.Spacing(15);

        // ✅ الشعار على اليسار
        row.ConstantItem(180).Image(logoBytesFooter).FitWidth();

        // ✅ معلومات الاتصال في المنتصف
        row.RelativeItem().Column(col =>
        {
            col.Spacing(4);

            col.Item().Row(r =>
            {
                r.Spacing(10);
                r.RelativeItem().Background("#B91C1C").Padding(5).Text("qatar@marinaplt.com").FontColor(Colors.White).FontFamily("Cairo").FontSize(12);
                r.RelativeItem().Background("#1E3A8A").Padding(5).Text("www.marinaplt.com").FontColor(Colors.White).FontFamily("Cairo").FontSize(12);
            });

            col.Item().Row(r =>
            {
                r.Spacing(10);
                r.RelativeItem().Background("#1E3A8A").Padding(5).Text("Tel.: 44 32 32 46").FontColor(Colors.White).FontFamily("Cairo").FontSize(12);
                r.RelativeItem().Background("#B91C1C").Padding(5).Text("Fax: 44 27 70 76").FontColor(Colors.White).FontFamily("Cairo").FontSize(12);
            });
        });
    });
            });
        });

        var pdf = document.GeneratePdf();
        return File(pdf , "application/pdf" , "report.pdf");
    }

    [HttpGet("GetReportPdf")]
    public IActionResult GetReportPdf(int Id)
    {
        QuestPDF.Settings.License=LicenseType.Community;

        var fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "Cairo-Regular.ttf");
        FontManager.RegisterFont(System.IO.File.OpenRead(fontPath));

        var reportDb = _db.Reports.Where(x => x.Id == Id).FirstOrDefault();


        var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "marina-logo.png");
        var logoBytes = System.IO.File.ReadAllBytes(logoPath);

        var logoPathFooter = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Picture1.jpg");
        var logoBytesFooter = System.IO.File.ReadAllBytes(logoPathFooter);

        var techPath = reportDb.TechSignaturePath?.TrimStart('/');
        var techImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", techPath);

        byte[] techImage = Array.Empty<byte>();
        if(System.IO.File.Exists(techImagePath))
        {
            techImage=System.IO.File.ReadAllBytes(techImagePath);
        }

        var clientPath = reportDb.ClientSignaturePath?.TrimStart('/');
        var clientImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", clientPath);

        byte[] clientImage = Array.Empty<byte>();
        if(System.IO.File.Exists(clientImagePath))
        {
            clientImage=System.IO.File.ReadAllBytes(clientImagePath);
        }


        var ReasonOfVisitJson = (reportDb.Installation != 0 ? reportDb.Installation + " Installation ," : "") +
                               (reportDb.Removing != 0 ? reportDb.Removing + " Removing ," : "") +
                               (reportDb.Shifting != 0 ? reportDb.Shifting + " Shifting ," : "") +
                               (reportDb.PeriodicMaintenance != 0 ? reportDb.PeriodicMaintenance + " PeriodicMaintenance ," : "") +
                               (reportDb.ThirdParty != 0 ? reportDb.ThirdParty + " ThirdParty ," : "") +
                               (reportDb.Inspection != 0 ? reportDb.Inspection + " Inspection ," : "") +
                               (reportDb.Delivery != 0 ? reportDb.Delivery + " Delivery ," : "") +
                               (reportDb.OnScaffolding != 0 ? reportDb.OnScaffolding + " OnScaffolding ," : "");
        var spareParts = ConvertSparePartsToString(reportDb.spareParts);



        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Size(PageSizes.A4);

                // ===== رأس الصفحة =====
                page.Header()
                    .Column(col =>
                    {
                        // 🖼️ الصورة بعرض الصفحة
                        col.Item()
                            .AlignCenter()
                            .Element(e =>
                            {
                                e.Image(logoBytes)
                                 .FitWidth();  // يجعل الصورة تمتد بعرض الصفحة تلقائيًا
                            });
                        col.Item().LineHorizontal(1)
    .LineColor(Colors.Grey.Lighten2);
                        // 📝 العنوان أسفل الصورة
                        col.Item()
                            .AlignCenter()
                            .PaddingTop(5)
                            .Text($"{reportDb.ReportType}")
                            .FontFamily("Cairo")
                            .FontSize(20)
                            .Bold();
                        col.Item().LineHorizontal(1)
    .LineColor(Colors.Grey.Lighten2);
                    });

                // ===== المحتوى =====
                page.Content()
                    .Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين العناصر
                            row.RelativeItem().Text($"Date : {reportDb.Date.ToShortDateString()}").FontFamily("Cairo").FontSize(12);
                            row.Spacing(60);
                            row.RelativeItem().Text($"Report # : {reportDb.ReportNumber}").FontFamily("Cairo").FontSize(12);
                        });

                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين العناصر
                            row.RelativeItem().Text($"Company Name : {reportDb.CompanyName}").FontFamily("Cairo").FontSize(12);
                        });
                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين العناصر
                            row.RelativeItem().Text($"Adress / Project : {reportDb.ProjectAddress}").FontFamily("Cairo").FontSize(12);
                        });

                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين العناصر
                            row.RelativeItem().Text($"Equipment : {reportDb.EquipmentType}").FontFamily("Cairo").FontSize(12);

                        });

                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين العناصر
                            row.RelativeItem().Text($"Specifications : {reportDb.Cradle} Cradle , {reportDb.Meter} Meter , With {reportDb.Unit} suspension Unit").FontFamily("Cairo").FontSize(14);

                        });

                        col.Item().LineHorizontal(1)
    .LineColor(Colors.Grey.Lighten2);

                        col.Item().Row(row =>
                        {
                            row.Spacing(15); // المسافة بين العناصر
                            row.RelativeItem().Text($"Model : {reportDb.ModelMarnia}    {reportDb.ModelMarniaHireOrSale}").FontFamily("Cairo").FontSize(12);

                        });


                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين العناصر
                            row.RelativeItem().Text($"Model : {reportDb.Model}                                S.N:{reportDb.SerialNumber}").FontFamily("Cairo").FontSize(14);

                        });


                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين العناصر
                            row.RelativeItem().Text($"Warranty : {reportDb.WarrantyStatus} ").FontFamily("Cairo").FontSize(12);

                        });

                        col.Item().LineHorizontal(1)
    .LineColor(Colors.Grey.Lighten2);

                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين العناصر
                            row.RelativeItem().Text($"Reason Of Visit : {ReasonOfVisitJson} ").FontFamily("Cairo").FontSize(12);

                        });
                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين العناصر
                            row.RelativeItem().Text($"Spare Parts : {spareParts} ").FontFamily("Cairo").FontSize(12);

                        });
                        col.Item().LineHorizontal(1)
    .LineColor(Colors.Grey.Lighten2);

                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين العناصر
                            row.RelativeItem().Text($"Report : {reportDb.Notes} ").FontFamily("Cairo").FontSize(12);

                        });
                        col.Item().LineHorizontal(1)
    .LineColor(Colors.Grey.Lighten2);
                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين العناصر
                            row.RelativeItem().Text($" ").FontFamily("Cairo").FontSize(12);

                        });
                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين العناصر
                            row.RelativeItem().Text($"All Workings have been reviewed at the site and there is no damaged happened due the installation, UN installation and maintenance and the machine is working in a good condition. Please check before leaving.\r\nتمت مراجعه كافه الاعمال بالموقع ولا توجد أى خسائر ناتجة عن أعمال الفك أو التركيب أو الصيانة التى تمت بالموقع والمعدة تعمل بحاله جيدة وشركة مارينا غير مسئوله عن اى ضرر يتم أكتشافة بعد مغادرة الموقع لذلك يرجى مراجعة مكان التركيب جيدا قبل مغادرة الفنيين\r\n ").FontFamily("Cairo").FontSize(12);

                        });
                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين العناصر
                            row.RelativeItem().Text($"PhoneNum. : {reportDb.PhoneNum} ").FontFamily("Cairo").FontSize(12);

                        });

                        col.Item().Row(row =>
                        {
                            row.Spacing(50); // المسافة بين العناصر
                            row.RelativeItem().Text($"Marina REP. : {reportDb.TechName} ").FontFamily("Cairo").FontSize(12);
                            //row.RelativeItem().Text($"PhoneNum. : {reportDb.PhoneNum} ").FontFamily("Cairo").FontSize(12);
                            row.RelativeItem().Text($"Site REP. : {reportDb.ClientName} ").FontFamily("Cairo").FontSize(12);

                        });
                        // صورة داخل المحتوى كمثال إضافي
                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين الصورتين

                            // الصورة الأولى
                            row.RelativeItem().Element(e =>
                            {
                                e.Border(1)
         .BorderColor(Colors.Grey.Darken2)
         .Padding(5)
         .Width(150)
         .Height(100)
         .Image(techImage)
         .FitWidth();
                            });

                            // الصورة الثانية
                            row.RelativeItem().Element(e =>
                            {
                                e.Border(1)
         .BorderColor(Colors.Grey.Darken2)
         .Padding(5)
         .Width(150)
                  .Height(100)
         .Image(clientImage) // استخدم صورة أخرى أو نفس الصورة
         .FitWidth();
                            });
                        });



                    });


                // ===== ذيل الصفحة =====
                page.Footer()
                    .BorderBottom(1)
    .PaddingVertical(10)
    .Row(row =>
    {
        row.Spacing(15);

        // ✅ الشعار على اليسار
        row.ConstantItem(180).Image(logoBytesFooter).FitWidth();

        // ✅ معلومات الاتصال في المنتصف
        row.RelativeItem().Column(col =>
        {
            col.Spacing(4);

            col.Item().Row(r =>
            {
                r.Spacing(10);
                r.RelativeItem().Background("#B91C1C").Padding(5).Text("qatar@marinaplt.com").FontColor(Colors.White).FontFamily("Cairo").FontSize(12);
                r.RelativeItem().Background("#1E3A8A").Padding(5).Text("www.marinaplt.com").FontColor(Colors.White).FontFamily("Cairo").FontSize(12);
            });

            col.Item().Row(r =>
            {
                r.Spacing(10);
                r.RelativeItem().Background("#1E3A8A").Padding(5).Text("Tel.: 44 32 32 46").FontColor(Colors.White).FontFamily("Cairo").FontSize(12);
                r.RelativeItem().Background("#B91C1C").Padding(5).Text("Fax: 44 27 70 76").FontColor(Colors.White).FontFamily("Cairo").FontSize(12);
            });
        });
    });
            });
        });

        var pdf = document.GeneratePdf();
        return File(pdf , "application/pdf" , "report.pdf");
    }
    private static string ConvertSparePartsToString(string? json)
    {
        if(string.IsNullOrWhiteSpace(json))
            return "";

        try
        {
            var parts = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, string>>>(json);
            if(parts==null)
                return "";

            var names = parts
                .Where(p => p.ContainsKey("partName"))
                .Select(p => p["partName"])
                .ToList();

            return string.Join(", " , names);
        }
        catch
        {
            return json; // return raw text if invalid JSON
        }
    }


    private string? GetFormValueOrDefault(IFormCollection form , string key)
    {
        if(form.TryGetValue(key , out var value)&&value.Count>0)
        {
            var stringValue = value.ToString();
            return string.IsNullOrWhiteSpace(stringValue) ? null : stringValue;
        }
        return null;
    }

}
