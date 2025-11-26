
using maria.Dto;
using maria.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Svg.FilterEffects;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using static maria.Dto.DeliveryReportDetailDto;
using static System.Net.Mime.MediaTypeNames;

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
        var currentYear = DateTime.Now.Year.ToString().Substring(2);

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
        var newReportNumber = $"{currentYear}/{nextNumber:D3}";

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

    [HttpPost("PostElevatorReport")]
    [RequestSizeLimit(20_000_000)] // 20 MB
    public async Task<IActionResult> PostElevatorReport()
    {
        var form = await Request.ReadFormAsync();
        var currentYear = DateTime.Now.Year.ToString().Substring(2);

        // ابحث عن آخر تقرير في نفس السنة
        var lastReport = _db.Elevator
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
        var newReportNumber = $"{currentYear}/{nextNumber:D3}";

        var report = new Elevator
        {
            // Basic Info
            Date = DateTime.TryParse(form["date"], out var parsedDate) ? parsedDate : DateTime.UtcNow,
            ReportNumber = newReportNumber,            //InvoiceNumber = form["invoiceNumber"],
            CompanyName = form["companyName"],
            salesName = form["personName"],
            ProjectAddress = GetFormValueOrDefault(form, "projectAddress"),
            widthShape = int.TryParse(form["width"], out var width) ? width : 0,
            heightShape = int.TryParse(form["height"], out var height) ? height : 0,
            radiusShape = int.TryParse(form["radius"], out var radius) ? radius : 0,
           // directionShape = int.TryParse(form["direction"], out var direction) ? direction : 0,
            floors = int.TryParse(form["floors"], out var floors) ? floors : 0,
            foundationHeight = int.TryParse(form["foundationHeight"], out var foundationHeight) ? foundationHeight : 0,
            resizableSquarewidth = int.TryParse(form["resizableSquarewidth"], out var resizableSquarewidth) ? resizableSquarewidth : 0,
            resizableSquareHeight = int.TryParse(form["resizableSquareHeight"], out var resizableSquareHeight) ? resizableSquareHeight : 0,

            wellStatus = form["wellStatus"],
            capinaStatus = form["capinaStatus"],
            directionWidth = int.TryParse(form["directionWidth"], out var directionWidth) ? directionWidth : 0,
            directionHeight = int.TryParse(form["directionHeight"], out var directionHeight) ? directionHeight : 0,
            liftWidth = int.TryParse(form["liftWidth"], out var liftWidth) ? liftWidth : 0,
            rightWidth = int.TryParse(form["rightWidth"], out var rightWidth) ? rightWidth : 0,
            centerWidth = int.TryParse(form["centerWidth"], out var centerWidth) ? centerWidth : 0,
            capinaHeight = int.TryParse(form["capinaHeight"], out var capinaHeight) ? capinaHeight : 0,



            floorHeights = form["floorHeights"],
            shapeType = form["shapeType"],
            typeElevator = form["typeElevator"],
            workRequied = form["workRequied"],
            doorDirections = form["doorDirections"],
            Notes = form["notes"],
            PhoneNum = form["phoneNum"],
            // Signatures (paths to be saved after upload)
          //  ClientSignaturePath = form["clientSignaturePath"],
           // TechSignaturePath = form["techSignaturePath"],
            reportType = form["reportType"],
            TechName = form["techName"],
            UserId = long.Parse(form["userId"]),


            CreatedAt = DateTime.UtcNow
        };


        // حفظ التوقيعات والصور في مجلد
        string uploadRoot = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "elevator");

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

            string relativePath = $"/elevator/{fileName}";

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

                case "images":
                    _db.ElevatorImage.Add(new ElevatorImage
                    {
                        FilePath=relativePath ,
                        FileName=file.FileName ,
                        Elevator=report
                    });
                    break;


                    //case "pdfFile":
                    //    report.PdfFilePath = relativePath;
                    //    break;
            }
        }
        if(report.shapeType=="square")
        {
            report.radiusShape=null;
        }
        else if(report.shapeType=="circle")
        {
            report.heightShape=null;
            report.widthShape=null;
        }

        _db.Elevator.Add(report);
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
    [HttpGet("safetyItem")]
    public IActionResult safetyItem()
    {
        var report = _db.SafetyItems.ToList();


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
    //[HttpGet("GetPagedElevatorReport")]
    //public async Task<IActionResult> GetPagedElevatorReport(long userId , int pageNumber = 1 , int pageSize = 10)
    //{
    //    if(pageNumber<1)
    //        pageNumber=1;
    //    if(pageSize<1)
    //        pageSize=10;


    //    var totalReports = await _db.Elevator.CountAsync();

    //    var reports = new List<Elevator>();

    //    if(userId>0)
    //    {
    //        reports=await _db.Elevator
    //           .Where(x => x.UserId==userId)
    //           .OrderByDescending(r => r.CreatedAt)
    //           .Skip((pageNumber-1)*pageSize)
    //           .Take(pageSize)
    //           .ToListAsync();
    //    }
    //    else
    //    {
    //        reports=await _db.Elevator
    //       .OrderByDescending(r => r.CreatedAt)
    //       .Skip((pageNumber-1)*pageSize)
    //       .Take(pageSize)
    //       .ToListAsync();
    //    }

    //    var imagesDb = await _db.ElevatorImage.Where(x=> reports.Select(y=>y.Id).Contains(x.ElevatorId) ).ToListAsync();

    //    var baseUrl = $"{Request.Scheme}://{Request.Host}";

    //    var pagedReports = reports.Select(x => new GetAllElevatorDto
    //    {
    //        Id = x.Id,
    //        Date = x.Date,
    //        ReportNumber = x.ReportNumber,
    //        typeElevator= x.typeElevator,
    //        InvoiceNumber = x.InvoiceNumber,
    //        CompanyName = x.CompanyName,
    //        ProjectAddress = x.ProjectAddress,
    //        shapeType = x.shapeType,
    //        widthShape = x.widthShape,
    //        heightShape = x.heightShape,
    //        radiusShape = x.radiusShape,
    //        directionShape = x.directionShape,
    //        floors = x.floors,
    //        foundationHeight = x.foundationHeight,
    //        // floors = x.floors,

    //        floorHeights = !string.IsNullOrEmpty(x.floorHeights)
    //? x.floorHeights.Trim('"', '[', ']').Replace("\",\"", ",")
    //: string.Empty ,
    //        workRequied = !string.IsNullOrEmpty(x.workRequied)
    //? x.workRequied.Trim('"', '[', ']').Replace("\",\"", ",")
    //: string.Empty ,
    //        Notes = x.Notes,
    //        CreatedAt = x.CreatedAt,
    //        ClientName = x.ClientName,
    //        TechName = x.TechName,
    //        PhoneNum = x.PhoneNum,
    //        ClientSignaturePath = baseUrl + x.ClientSignaturePath,
    //        TechSignaturePath = baseUrl + x.TechSignaturePath,
    //        Images = imagesDb
    //            .Where(y => y.ElevatorId == x.Id)
    //            .Select(p => baseUrl + p.FilePath)
    //            .ToList()
    //    }).ToList();

    //    return Ok(new
    //    {
    //        totalCount = totalReports ,
    //        pageNumber ,
    //        pageSize ,
    //        totalPages = (int)Math.Ceiling(totalReports/(double)pageSize) ,
    //        reports = pagedReports
    //    });
    //}
    [HttpGet("GetPagedSiteReports")]
    public async Task<IActionResult> GetPagedSiteReports(long userId , int page = 1 , int pageSize = 5)
    {
        var query = _db.SiteReports.AsQueryable();
        if(userId>0)
        {
            query=_db.SiteReports
         .Include(x => x.checkingItemReport).Where(x => x.UserId==userId)
         .OrderByDescending(x => x.Date);
        }
        else
        {
            query=_db.SiteReports
          .Include(x => x.checkingItemReport)
          .OrderByDescending(x => x.Date);
        }

        var reportlist =  await query
        .Skip((page-1)*pageSize)
        .Take(pageSize).ToListAsync();


        var imagesDb = await _db.SiteReportImages
        .Where(i => reportlist.Select(x=>x.Id).Contains(i.siteReportId))
        .ToListAsync();


        var baseUrl = $"{Request.Scheme}://{Request.Host}";


        int totalCount = await query.CountAsync();
        var reports =  reportlist
        .Select(x => new SiteReportDto
        {
            Id = x.Id,
            CompanyName = x.CompanyName,
            Date = x.Date,
            ClientSignaturePath = baseUrl + x.ClientSignaturePath,
            TechSignaturePath = baseUrl + x.TechSignaturePath,
            CheckingItemsCount = x.checkingItemReport.Count,
            ReportNumber = x.ReportNumber,
            ClientName = x.ClientName,
            TechName = x.TechName,
            Images = imagesDb !=null ? imagesDb
                .Where(y => y.siteReportId == x.Id)
                .Select(p => baseUrl + p.FilePath)
                .ToList():null

        })
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

    [HttpGet("GetPagedSafetyReports")]
    public async Task<IActionResult> GetPagedSafetyReports(long userId , int page = 1 , int pageSize = 5)
    {
        var query = _db.SafetyReport.AsQueryable();
        if(userId>0)
        {
            query=_db.SafetyReport
         .Include(x => x.safetyItemsReport).Where(x => x.UserId==userId)
         .OrderByDescending(x => x.Date);
        }
        else
        {
            query=_db.SafetyReport
          .Include(x => x.safetyItemsReport)
          .OrderByDescending(x => x.Date);
        }


        var reportlist =  await query
        .Skip((page-1)*pageSize)
        .Take(pageSize).ToListAsync();


        var imagesDb = await _db.SafetyReportImage
        .Where(i => reportlist.Select(x=>x.Id).Contains(i.safetyReportId))
        .ToListAsync();


        var baseUrl = $"{Request.Scheme}://{Request.Host}";


        int totalCount = await query.CountAsync();
        var reports =  reportlist
        .Select(x => new SafetyReportDto
        {
            Id = x.Id,
            CompanyName = x.CompanyName,
            Date = x.Date,
            CreatedAt = x.CreatedAt,
            ClientSignaturePath = baseUrl + x.ClientSignaturePath,
            TechSignaturePath = baseUrl + x.TechSignaturePath,
            safetyItemsCount = x.safetyItemsReport.Count,
            ReportNumber = x.ReportNumber,
            ClientName = x.ClientName,
            TechName = x.TechName,
            PhoneNum = x.PhoneNum,
            ProjectName = x.ProjectName,
            ProjectDescription = x.ProjectDescription,
            Projectlocation = x.Projectlocation,
            TeamNum= x.TeamNum,
            TeamLeaderName = x.TeamLeaderName,
            TeamLeaderNum = x.TeamLeaderNum,
            TeamMembers = x.TeamMembers,
            Notes = x.Notes,
            SiteName= x.SiteName,
            Images = imagesDb !=null ? imagesDb
                .Where(y => y.safetyReportId == x.Id)
                .Select(p => baseUrl + p.FilePath)
                .ToList():null

        })
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

        var result = new SiteReportDetailDto
        {
            CompanyName=report.CompanyName ,
            Date=report.Date ,
            ClientSignaturePath=report.ClientSignaturePath!=null ? baseUrl+report.ClientSignaturePath : null ,
            TechSignaturePath=baseUrl+report.TechSignaturePath!=null ? baseUrl+report.TechSignaturePath : null ,
            checkingItems=checkItemDb.Select(a =>
            {

                var reportItem = report.checkingItemReport.Where(x => x.CheckingItemId==a.Id).FirstOrDefault();


                return new CheckingItemsDto
                {
                    Item=a.Item ,
                    fault=reportItem?.fault ,
                    CorrectiveAction=reportItem?.CorrectiveAction ,
                    faultFlag=reportItem?.faultFlag??false ,
                    CorrectiveActionFlag=reportItem?.CorrectiveActionFlag??false ,
                    Review=!(reportItem?.faultFlag??false)&&!(reportItem?.CorrectiveActionFlag??false)
                };
            }).ToList()
        };
        return Ok(result);

    }


    [HttpGet("GetSafetyReportDetails/{id}")]
    public async Task<IActionResult> GetSafetyReportDetails(int id)
    {
        var report = await _db.SafetyReport
        .Include(x => x.safetyItemsReport)
        .FirstOrDefaultAsync(x => x.Id == id);

        var checkItemDb = await _db.SafetyItems.ToListAsync();

        if(report==null)
            return NotFound();
        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        var result = new SafetyReportDetailDto
        {
            CompanyName=report.CompanyName ,
            Date=report.Date ,
            ClientSignaturePath=report.ClientSignaturePath!=null ? baseUrl+report.ClientSignaturePath : null ,
            TechSignaturePath=baseUrl+report.TechSignaturePath!=null ? baseUrl+report.TechSignaturePath : null ,
            checkingItems=checkItemDb.Select(a =>
            {

                var reportItem = report.safetyItemsReport.Where(x => x.SafetyItemsId==a.Id).FirstOrDefault();


                return new CheckingSafetyItemsDto
                {
                    Item=a.Item ,
                    CorrectiveAction=reportItem?.CorrectiveAction ,
                    faultFlag=reportItem?.faultFlag??false ,
                    Review=!(reportItem?.faultFlag??false),
                };
            }).ToList()
        };
        return Ok(result);

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
                Description=a.deliveryNote.Description ,
                DeliveryType=a.deliveryNote.DeliveryType ,
                Quantity=a.Quantity ,
                Unit=a.UnitValue!=null ? a.UnitValue : null
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



            var currentYear = DateTime.Now.Year.ToString().Substring(2);

            // ابحث عن آخر تقرير في نفس السنة
            var lastReport = _db.SiteReports
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
            var newReportNumber = $"{currentYear}/{nextNumber:D3}";

            var sitereport = new SiteReport
            {
                CompanyName = request["companyName"],
                ReportNumber = newReportNumber,
                TechName = request["techName"],
                ClientName = request["clientName"],
                UserId = long.Parse(request["userId"]),
                Date = DateTime.TryParse(request["date"], out var parsedDate) ? parsedDate : DateTime.Now,
                PhoneNum = request["phoneNum"],
                CreatedAt = DateTime.Now
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
                    FilePath=$"/UploadSiteReport/{fileName}"

                });
                await _db.SaveChangesAsync();

            }




            // تحويل العناصر القادمة من JSON إلى كائنات
            foreach(var item in Items)
            {
                if(item.faultFlag==true||item.CorrectiveActionFlag==true)
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
                    //if(string.IsNullOrEmpty(item.Fault)&&string.IsNullOrEmpty(item.CorrectiveAction)&&item.faultFlag==true&&item.CorrectiveActionFlag==item.faultFlag==true)
                    //    continue;

                    _db.CheckingItemReports.Add(report);
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

    [HttpPost("addsafetyReport")]
    public async Task<IActionResult> addsafetyReport([FromForm] IFormCollection request)
    {
        try
        {

            var itemsJson = request["items"];
            if(string.IsNullOrEmpty(itemsJson))
                return BadRequest("No items data received.");


            // var Items = System.Text.Json.JsonSerializer.Deserialize<List<CheckingItemDto>>(itemsJson)!;


            var Items = System.Text.Json.JsonSerializer.Deserialize<List<SafteyItemDto>>(
    itemsJson,
    new System.Text.Json.JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    }
)!;

            string uploadRoot = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "Safety");

            if(!Directory.Exists(uploadRoot))
                Directory.CreateDirectory(uploadRoot);

            // حفظ الصور

            // ✅ حفظ الصور





            // حفظ التواقيع
            string? clientSignaturePath = null;
            string? techSignaturePath = null;
            var clientSig = request.Files.FirstOrDefault(f => f.Name == "clientSignature");
            var techSig = request.Files.FirstOrDefault(f => f.Name == "techSignature");



            var currentYear = DateTime.Now.Year.ToString().Substring(2);

            // ابحث عن آخر تقرير في نفس السنة
            var lastReport = _db.SafetyReport
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
            var newReportNumber = $"{currentYear}/{nextNumber:D3}";

            var saftyreport = new SafetyReport
            {
                CompanyName = request["companyName"],
                ReportNumber = newReportNumber,
                TechName = request["techName"],
                ClientName = request["clientName"],
                UserId = long.Parse(request["userId"]),
                Date = DateTime.TryParse(request["date"], out var parsedDate) ? parsedDate : DateTime.Now,
                PhoneNum = request["phoneNum"],
                CreatedAt = DateTime.Now,
                SiteName = request["siteName"],
                TeamNum = int.Parse(request["teamNum"]),
                TeamLeaderName = request["teamLeaderName"],
                TeamLeaderNum = int.Parse(request["teamLeaderNum"]),
                ProjectDescription = request["projectDescription"],
                Projectlocation = request["projectlocation"],
                ProjectName = request["projectName"],
                TeamMembers = request["teamMembers"],
                Notes = request["notes"],
            };

            if(clientSig!=null)
            {
                string fileName = $"client_{Guid.NewGuid()}.png";
                string fullPath = Path.Combine(uploadRoot, fileName);
                using(var stream = new FileStream(fullPath , FileMode.Create))
                    await clientSig.CopyToAsync(stream);
                clientSignaturePath=$"/Safety/{fileName}";
                saftyreport.ClientSignaturePath=clientSignaturePath;
            }

            if(techSig!=null)
            {
                string fileName = $"tech_{Guid.NewGuid()}.png";
                string fullPath = Path.Combine(uploadRoot, fileName);
                using(var stream = new FileStream(fullPath , FileMode.Create))
                    await techSig.CopyToAsync(stream);
                techSignaturePath=$"/Safety/{fileName}";
                saftyreport.TechSignaturePath=techSignaturePath;
            }


            _db.SafetyReport.Add(saftyreport);
            await _db.SaveChangesAsync();

            List<string> imagePaths = new List<string>();
            foreach(var file in request.Files.Where(f => f.Name=="images"))
            {
                string fileName = $"{Guid.NewGuid()}_{file.FileName}";
                string fullPath = Path.Combine(uploadRoot, fileName);
                using(var stream = new FileStream(fullPath , FileMode.Create))
                    await file.CopyToAsync(stream);
                imagePaths.Add($"/Safety/{fileName}");
                _db.SafetyReportImage.Add(new SafetyReportImage
                {
                    safetyReportId=saftyreport.Id ,
                    FileName=fileName ,
                    FilePath=$"/Safety/{fileName}"

                });
                await _db.SaveChangesAsync();

            }




            // تحويل العناصر القادمة من JSON إلى كائنات
            foreach(var item in Items)
            {
                if(item.faultFlag==true||!string.IsNullOrEmpty(item.CorrectiveAction))
                {
                    var report = new SafetyItemsReport
                    {
                        SafetyItemsId = item.SafetyItemId,
                        CorrectiveAction = item.CorrectiveAction,
                        faultFlag = item.faultFlag,
                        SafetyReportId = saftyreport.Id
                    };
                    //if(string.IsNullOrEmpty(item.Fault)&&string.IsNullOrEmpty(item.CorrectiveAction)&&item.faultFlag==true&&item.CorrectiveActionFlag==item.faultFlag==true)
                    //    continue;

                    _db.SafetyItemsReport.Add(report);
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

    [HttpPost("DeliveryNoteReportList")]
    public async Task<IActionResult> DeliveryNoteReportList([FromForm] IFormCollection request)
    {
        try
        {
            var currentYear = DateTime.Now.Year.ToString().Substring(2);

            // ابحث عن آخر تقرير في نفس السنة
            var lastReport = _db.DeliveryReport
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
            var newReportNumber = $"{currentYear}/{nextNumber:D3}";



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
                    FilePath=$"/UploadSiteReport/{fileName}"

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
    public async Task<IActionResult> GetPagedDeliveryReports(long userId , int page = 1 , int pageSize = 5)
    {
        var query = _db.DeliveryReport.AsQueryable();
        if(userId>0)
        {
            query=_db.DeliveryReport.Where(x => x.UserId==userId)
       .Include(x => x.checkingItemReport)
       .OrderByDescending(x => x.Date);
        }
        else
        {
            query=_db.DeliveryReport
       .Include(x => x.checkingItemReport)
       .OrderByDescending(x => x.Date);
        }


        var reportlist =  await query
        .Skip((page-1)*pageSize)
        .Take(pageSize).ToListAsync();


        var imagesDb = await _db.DelivryReportImages
        .Where(i => reportlist.Select(x=>x.Id).Contains(i.deliveryReportId))
        .ToListAsync();


        var baseUrl = $"{Request.Scheme}://{Request.Host}";


        int totalCount = await query.CountAsync();
        var reports =  reportlist
        .Select(x => new SiteReportDto
        {
            Id = x.Id,
            CompanyName = x.CompanyName,
            Date = x.Date,
            ClientSignaturePath = baseUrl + x.ClientSignaturePath,
            TechSignaturePath = baseUrl + x.TechSignaturePath,
            CheckingItemsCount = x.checkingItemReport.Count,
            ReportNumber = x.ReportNumber,
            ClientName = x.ClientName,
            TechName = x.TechName,
            Images = imagesDb !=null ? imagesDb
                .Where(y => y.deliveryReportId == x.Id)
                .Select(p => baseUrl + p.FilePath)
                .ToList():null

        })
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
    public IActionResult GetReportPdf23(int Id , string InvoiceNum = "  ")
    {
        QuestPDF.Settings.License=LicenseType.Community;

        var fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "Cairo-Regular.ttf");
        FontManager.RegisterFont(System.IO.File.OpenRead(fontPath));

        var reportDb = _db.Reports.Where(x=>x.Id == Id).FirstOrDefault();


        var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Picture1.jpg");
        var logoBytes = System.IO.File.ReadAllBytes(logoPath);

        var sealPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "seal.png");
        var sealBytes = System.IO.File.ReadAllBytes(sealPath);

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
                page.Margin(10);
                page.Size(PageSizes.A4);

                // ===== رأس الصفحة =====
                page.Header()
                    // 🖼️ الصورة بعرض الصفحة
                         .Column(col =>
                          {
                              // 🖼️ الصورة بعرض الصفحة
                              col.Item()
                            .AlignCenter()
                            .Element(e =>
                            {

                                e. Width(150)
                                 .Height(75)
                                .Image(logoBytes)
                                 .FitWidth()
                                 ;  // يجعل الصورة تمتد بعرض الصفحة تلقائيًا
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
                        col.Item().Layers(layers =>
                        {
                            // ✅ الطبقة الأساسية (التواقيع)
                            layers.PrimaryLayer().Row(row =>
                            {
                                row.Spacing(15);

                                // الصورة الأولى (توقيع الفني)
                                row.RelativeItem().Element(e =>
                                {

                                    e.Padding(5)
             .Width(150)
             .Height(100)
             .Image(techImage)
             .FitWidth();
                                });

                                // الصورة الثانية (توقيع العميل)
                                row.RelativeItem().Element(e =>
                                {

                                    e .Padding(5)
             .Width(150)
             .Height(100)
             .Image(clientImage)
             .FitWidth();
                                });
                            });

                            // ✅ الطبقة الثانية (الختم فوق الصورتين)
                            layers.Layer()
        .AlignCenter()
        .AlignMiddle()
        .Element(e =>
        {
            e.Width(100)         // ← الحجم يوضع هنا (على الـ container)
             .Image(sealBytes)   // ← وأخيرًا الصورة
             .FitWidth();
        });
                        });



                    });


                // ===== ذيل الصفحة =====
                page.Footer()
                    .BorderBottom(1)
    .PaddingVertical(2)
    .Row(row =>
    {
        row.Spacing(10);

        // ✅ الشعار على اليسار
        row.ConstantItem(150).Image(logoBytesFooter).FitWidth();

        // ✅ معلومات الاتصال في المنتصف
        row.RelativeItem().Column(col =>
        {
            col.Spacing(6);

            col.Item().Row(r =>
            {
                r.Spacing(6);
                r.RelativeItem().Background("#B91C1C").Padding(5).Text("qatar@marinaplt.com").FontColor(Colors.White).FontFamily("Cairo").FontSize(12);
                r.RelativeItem().Background("#1E3A8A").Padding(5).Text("www.marinaplt.com").FontColor(Colors.White).FontFamily("Cairo").FontSize(12);
            });

            col.Item().Row(r =>
            {
                r.Spacing(6);
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


    [HttpGet("pdf24")]
    public IActionResult GetReportPdf24(int Id , string InvoiceNum = " ")
    {
        QuestPDF.Settings.License=LicenseType.Community;

        var fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "Cairo-Regular.ttf");
        FontManager.RegisterFont(System.IO.File.OpenRead(fontPath));

        var SiteReportDb = _db.SiteReports.Where(x=>x.Id == Id).Include(y=>y.checkingItemReport).FirstOrDefault();


        var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Picture1.jpg");
        var logoBytes = System.IO.File.ReadAllBytes(logoPath);

        var sealPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "seal.png");
        var sealBytes = System.IO.File.ReadAllBytes(sealPath);

        var logoPathFooter = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Picture1.jpg");
        var logoBytesFooter = System.IO.File.ReadAllBytes(logoPathFooter);

        var techPath = SiteReportDb.TechSignaturePath?.TrimStart('/');
        var techImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", techPath);

        byte[] techImage = Array.Empty<byte>();
        if(System.IO.File.Exists(techImagePath))
        {
            techImage=System.IO.File.ReadAllBytes(techImagePath);
        }

        var clientPath = SiteReportDb.ClientSignaturePath?.TrimStart('/');
        var clientImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", clientPath);

        byte[] clientImage = Array.Empty<byte>();
        if(System.IO.File.Exists(clientImagePath))
        {
            clientImage=System.IO.File.ReadAllBytes(clientImagePath);
        }

        var checkItemDb =  _db.CheckingItems.ToList();
        var  checkingItems=checkItemDb.Select(a =>
        {

            var reportItem = SiteReportDb.checkingItemReport.Where(x => x.CheckingItemId==a.Id).FirstOrDefault();


            return new CheckingItemsDto
            {
                Item=a.Item ,
                fault=reportItem?.fault ,
                CorrectiveAction=reportItem?.CorrectiveAction ,
                faultFlag=reportItem?.faultFlag??false ,
                CorrectiveActionFlag=reportItem?.CorrectiveActionFlag??false ,
                Review=!(reportItem?.faultFlag??false)&&!(reportItem?.CorrectiveActionFlag??false)
            };
        }).ToList() ;







        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(10);
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

                                e. Width(150)
                                 .Height(75)
                                .Image(logoBytes)
                                 .FitWidth()
                                 ;  // يجعل الصورة تمتد بعرض الصفحة تلقائيًا
                            });
                        col.Item().LineHorizontal(1)
    .LineColor(Colors.Grey.Lighten2);
                        // 📝 العنوان أسفل الصورة
                        col.Item()
                            .AlignCenter()
                            .PaddingTop(1)
                            .Text("Site Report")
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
                            row.RelativeItem().Text($"Date : {SiteReportDb.Date.ToShortDateString()}").FontFamily("Cairo").FontSize(12);
                            row.Spacing(60);
                            row.RelativeItem().Text($"Report # : {SiteReportDb.ReportNumber}").FontFamily("Cairo").FontSize(12);
                            row.RelativeItem().Text($"Invoice # : {InvoiceNum}").FontFamily("Cairo").FontSize(12);
                        });

                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين العناصر
                            row.RelativeItem().Text($"Company Name : {SiteReportDb.CompanyName}").FontFamily("Cairo").FontSize(12);
                        });



                        //col.Item().Row(row =>
                        //{
                        //    row.Spacing(20); // المسافة بين العناصر
                        //    row.RelativeItem().Text($"Report : {reportDb.Notes} ").FontFamily("Cairo").FontSize(12);

                        //});
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(60);
                                columns.RelativeColumn(8);
                                columns.RelativeColumn(5);
                                columns.RelativeColumn(11);
                                columns.RelativeColumn(8);
                                columns.RelativeColumn(8);
                            });

                            // ===== هيدر الجدول =====
                            table.Header(header =>
                            {
                                header.Cell().Border(1).Background("#f0f0f0").Padding(1) .AlignMiddle().AlignCenter().Text("Items").FontFamily("Cairo").FontSize(8).Bold();
                                header.Cell().Border(1).Background("#f0f0f0").Padding(1).AlignMiddle().AlignCenter().Text("Review").FontFamily("Cairo").FontSize(8).Bold();
                                header.Cell().Border(1).Background("#f0f0f0").Padding(1).AlignMiddle().AlignCenter().Text("Fault").FontFamily("Cairo").FontSize(8).Bold();
                                header.Cell().Border(1).Background("#f0f0f0").Padding(1).AlignMiddle().AlignCenter().Text("Corrective").FontFamily("Cairo").FontSize(8).Bold();
                                header.Cell().Border(1).Background("#f0f0f0").Padding(1).AlignMiddle().AlignCenter().Text("Fault").FontFamily("Cairo").FontSize(8).Bold();
                                header.Cell().Border(1).Background("#f0f0f0").Padding(1).AlignMiddle().AlignCenter().Text("Corrective").FontFamily("Cairo").FontSize(8).Bold();
                            });
                            foreach (var item in checkingItems)
                            {
                                table.Cell().Border(1).PaddingVertical(1).PaddingHorizontal(4).AlignMiddle().AlignCenter()
                             .Text(item.Item ).FontFamily("Cairo").FontSize(9);

                                table.Cell().Border(1).PaddingVertical(1).PaddingHorizontal(4).AlignMiddle().AlignCenter()
                             .Text(item.Review == true ? "✔" : " ").FontFamily("Cairo").FontSize(10);

                                table.Cell().Border(1).PaddingVertical(1).PaddingHorizontal(4).AlignMiddle().AlignCenter()
                             .Text(item.faultFlag == true ? "✔" : " ").FontFamily("Cairo").FontSize(10);

                                table.Cell().Border(1).PaddingVertical(1).PaddingHorizontal(4).AlignMiddle().AlignCenter()
                             .Text(item.CorrectiveActionFlag == true ? "✔" : "  ").FontFamily("Cairo").FontSize(9);



                                table.Cell().Border(1).PaddingVertical(1).PaddingHorizontal(4).AlignMiddle().AlignCenter()
                             .Text(item.fault ?? "-").FontFamily("Cairo").FontSize(9);
                                table.Cell().Border(1).PaddingVertical(1).PaddingHorizontal(4).AlignMiddle().AlignCenter()
                             .Text(item.CorrectiveAction ?? "-").FontFamily("Cairo").FontSize(9);



                            }



                        });

                        //  col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);







                        col.Item().Row(row =>
                        {
                            row.Spacing(15); // المسافة بين العناصر
                            row.RelativeItem().Text($"PhoneNum. : {SiteReportDb.PhoneNum} ").FontFamily("Cairo").FontSize(12);

                        });

                        col.Item().Row(row =>
                        {
                            row.Spacing(50); // المسافة بين العناصر
                            row.RelativeItem().Text($"Marina REP. : {SiteReportDb.TechName} ").FontFamily("Cairo").FontSize(12);
                            //row.RelativeItem().Text($"PhoneNum. : {reportDb.PhoneNum} ").FontFamily("Cairo").FontSize(12);
                            row.RelativeItem().Text($"Site REP. : {SiteReportDb.ClientName} ").FontFamily("Cairo").FontSize(12);

                        });
                        // صورة داخل المحتوى كمثال إضافي
                        col.Item().Layers(layers =>
                        {
                            // ✅ الطبقة الأساسية (التواقيع)
                            layers.PrimaryLayer().Row(row =>
                            {
                                row.Spacing(15);

                                // الصورة الأولى (توقيع الفني)
                                row.RelativeItem().Element(e =>
                                {

                                    e.Padding(5)
             .Width(150)
             .Height(100)
             .Image(techImage)
             .FitWidth();
                                });

                                // الصورة الثانية (توقيع العميل)
                                row.RelativeItem().Element(e =>
                                {

                                    e.Padding(5)
             .Width(150)
             .Height(100)
             .Image(clientImage)
             .FitWidth();
                                });
                            });

                            // ✅ الطبقة الثانية (الختم فوق الصورتين)
                            layers.Layer()
        .AlignCenter()
        .AlignMiddle()
        .Element(e =>
        {
            e.Width(100)         // ← الحجم يوضع هنا (على الـ container)
             .Image(sealBytes)   // ← وأخيرًا الصورة
             .FitWidth();
        });
                        });





                    });


                // ===== ذيل الصفحة =====
                page.Footer()
                    .BorderBottom(1)
    .PaddingVertical(2)
    .Row(row =>
    {
        row.Spacing(10);

        // ✅ الشعار على اليسار
        row.ConstantItem(150).Image(logoBytesFooter).FitWidth();

        // ✅ معلومات الاتصال في المنتصف
        row.RelativeItem().Column(col =>
        {
            col.Spacing(6);

            col.Item().Row(r =>
            {
                r.Spacing(6);
                r.RelativeItem().Background("#B91C1C").Padding(5).Text("qatar@marinaplt.com").FontColor(Colors.White).FontFamily("Cairo").FontSize(12);
                r.RelativeItem().Background("#1E3A8A").Padding(5).Text("www.marinaplt.com").FontColor(Colors.White).FontFamily("Cairo").FontSize(12);
            });

            col.Item().Row(r =>
            {
                r.Spacing(6);
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
    [HttpGet("GetSiteReportPdf")]
    public IActionResult GetSiteReportPdf(int Id)
    {
        QuestPDF.Settings.License=LicenseType.Community;

        var fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "Cairo-Regular.ttf");
        FontManager.RegisterFont(System.IO.File.OpenRead(fontPath));

        var SiteReportDb = _db.SiteReports.Where(x=>x.Id == Id).Include(y=>y.checkingItemReport).FirstOrDefault();


        var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "marina-logo.png");
        var logoBytes = System.IO.File.ReadAllBytes(logoPath);

        var logoPathFooter = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Picture1.jpg");
        var logoBytesFooter = System.IO.File.ReadAllBytes(logoPathFooter);

        var techPath = SiteReportDb.TechSignaturePath?.TrimStart('/');
        var techImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", techPath);

        byte[] techImage = Array.Empty<byte>();
        if(System.IO.File.Exists(techImagePath))
        {
            techImage=System.IO.File.ReadAllBytes(techImagePath);
        }

        var clientPath = SiteReportDb.ClientSignaturePath?.TrimStart('/');
        var clientImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", clientPath);

        byte[] clientImage = Array.Empty<byte>();
        if(System.IO.File.Exists(clientImagePath))
        {
            clientImage=System.IO.File.ReadAllBytes(clientImagePath);
        }

        var checkItemDb =  _db.CheckingItems.ToList();
        var  checkingItems=checkItemDb.Select(a =>
        {

            var reportItem = SiteReportDb.checkingItemReport.Where(x => x.CheckingItemId==a.Id).FirstOrDefault();


            return new CheckingItemsDto
            {
                Item=a.Item ,
                fault=reportItem?.fault ,
                CorrectiveAction=reportItem?.CorrectiveAction ,
                faultFlag=reportItem?.faultFlag??false ,
                CorrectiveActionFlag=reportItem?.CorrectiveActionFlag??false ,
                Review=!(reportItem?.faultFlag??false)&&!(reportItem?.CorrectiveActionFlag??false)
            };
        }).ToList() ;








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
                            .Text("Site Report")
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
                            row.RelativeItem().Text($"Date : {SiteReportDb.Date.ToShortDateString()}").FontFamily("Cairo").FontSize(12);
                            row.Spacing(60);
                            row.RelativeItem().Text($"Report # : {SiteReportDb.ReportNumber}").FontFamily("Cairo").FontSize(12);
                            row.RelativeItem().Text($"Invoice # :").FontFamily("Cairo").FontSize(12);
                        });

                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين العناصر
                            row.RelativeItem().Text($"Company Name : {SiteReportDb.CompanyName}").FontFamily("Cairo").FontSize(12);
                        });



                        //col.Item().Row(row =>
                        //{
                        //    row.Spacing(20); // المسافة بين العناصر
                        //    row.RelativeItem().Text($"Report : {reportDb.Notes} ").FontFamily("Cairo").FontSize(12);

                        //});
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(60);
                                columns.RelativeColumn(8);
                                columns.RelativeColumn(8);
                                columns.RelativeColumn(8);
                                columns.RelativeColumn(8);
                                columns.RelativeColumn(8);
                            });

                            // ===== هيدر الجدول =====
                            table.Header(header =>
                            {
                                header.Cell().Border(1).Background("#f0f0f0").Padding(2).Text("Items").FontFamily("Cairo").Bold();
                                header.Cell().Border(1).Background("#f0f0f0").Padding(2).Text("Review").FontFamily("Cairo").Bold();
                                header.Cell().Border(1).Background("#f0f0f0").Padding(2).Text("Fault").FontFamily("Cairo").Bold();
                                header.Cell().Border(1).Background("#f0f0f0").Padding(2).Text("Corrective").FontFamily("Cairo").Bold();
                                header.Cell().Border(1).Background("#f0f0f0").Padding(2).Text("Fault").FontFamily("Cairo").Bold();
                                header.Cell().Border(1).Background("#f0f0f0").Padding(2).Text("Corrective").FontFamily("Cairo").Bold();
                            });
                            foreach (var item in checkingItems)
                            {
                                table.Cell().Border(1).PaddingVertical(1).PaddingHorizontal(4)
                             .Text(item.Item ).FontFamily("Cairo").FontSize(9);

                                table.Cell().Border(1).PaddingVertical(1).PaddingHorizontal(4)
                             .Text(item.Review == true ? "✔" : " ").FontFamily("Cairo").FontSize(10);

                                table.Cell().Border(1).PaddingVertical(1).PaddingHorizontal(4)
                             .Text(item.faultFlag == true ? "✔" : " ").FontFamily("Cairo").FontSize(10);

                                table.Cell().Border(1).PaddingVertical(1).PaddingHorizontal(4)
                             .Text(item.CorrectiveActionFlag == true ? "✔" : "  ").FontFamily("Cairo").FontSize(9);



                                table.Cell().Border(1).PaddingVertical(1).PaddingHorizontal(4)
                             .Text(item.fault ?? "-").FontFamily("Cairo").FontSize(9);
                                table.Cell().Border(1).PaddingVertical(1).PaddingHorizontal(4)
                             .Text(item.CorrectiveAction ?? "-").FontFamily("Cairo").FontSize(9);



                            }



                        });

                        //  col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);







                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين العناصر
                            row.RelativeItem().Text($"PhoneNum. : {SiteReportDb.PhoneNum} ").FontFamily("Cairo").FontSize(12);

                        });

                        col.Item().Row(row =>
                        {
                            row.Spacing(50); // المسافة بين العناصر
                            row.RelativeItem().Text($"Marina REP. : {SiteReportDb.TechName} ").FontFamily("Cairo").FontSize(12);
                            //row.RelativeItem().Text($"PhoneNum. : {reportDb.PhoneNum} ").FontFamily("Cairo").FontSize(12);
                            row.RelativeItem().Text($"Site REP. : {SiteReportDb.ClientName} ").FontFamily("Cairo").FontSize(12);

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
    .PaddingVertical(5)
    .Row(row =>
    {
        row.Spacing(10);

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

    [HttpGet("GetSafetyReportPdf")]
    public IActionResult GetSafetyReportPdf(int Id , string InvoiceNum = " ")
    {
        QuestPDF.Settings.License=LicenseType.Community;

        var fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "Cairo-Regular.ttf");
        FontManager.RegisterFont(System.IO.File.OpenRead(fontPath));

        var SiteReportDb = _db.SafetyReport.Where(x=>x.Id == Id).Include(y=>y.safetyItemsReport).FirstOrDefault();



        var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Picture1.jpg");
        var logoBytes = System.IO.File.ReadAllBytes(logoPath);

        var sealPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "seal.png");
        var sealBytes = System.IO.File.ReadAllBytes(sealPath);

        var logoPathFooter = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Picture1.jpg");
        var logoBytesFooter = System.IO.File.ReadAllBytes(logoPathFooter);

        var techPath = SiteReportDb.TechSignaturePath?.TrimStart('/');
        var techImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", techPath);

        byte[] techImage = Array.Empty<byte>();
        if(System.IO.File.Exists(techImagePath))
        {
            techImage=System.IO.File.ReadAllBytes(techImagePath);
        }

        var clientPath = SiteReportDb.ClientSignaturePath?.TrimStart('/');
        var clientImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", clientPath);

        byte[] clientImage = Array.Empty<byte>();
        if(System.IO.File.Exists(clientImagePath))
        {
            clientImage=System.IO.File.ReadAllBytes(clientImagePath);
        }



        var checkItemDb =  _db.CheckingItems.ToList();
        var  checkingItems=checkItemDb.Select(a =>
        {

            var reportItem = SiteReportDb.safetyItemsReport.Where(x => x.SafetyItemsId==a.Id).FirstOrDefault();


            return new CheckingSafetyItemsDto
            {
                Item=a.Item ,
                CorrectiveAction=reportItem?.CorrectiveAction ,
                faultFlag=reportItem?.faultFlag??false ,
                Review=!(reportItem?.faultFlag??false)
            };
        }).ToList() ;




        var members = !string.IsNullOrEmpty(SiteReportDb.TeamMembers)
                    ?SiteReportDb.TeamMembers.Trim('"', '[', ']').Replace("\",\"", ",")
                    : string.Empty;



        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(10);
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

                                e. Width(100)
                                 .Height(50)
                                .Image(logoBytes)
                                 .FitWidth()
                                 ;  // يجعل الصورة تمتد بعرض الصفحة تلقائيًا
                            });
                        col.Item().LineHorizontal(1)
    .LineColor(Colors.Grey.Lighten2);
                        // 📝 العنوان أسفل الصورة
                        col.Item()
                            .AlignCenter()
                            .PaddingTop(5)
                            .Text("Safety Report")
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
                            row.Spacing(10); // المسافة بين العناصر
                            row.RelativeItem().Text($"Date : {SiteReportDb.Date.ToShortDateString()}").FontFamily("Cairo").FontSize(10);
                            row.Spacing(20);
                            row.RelativeItem().Text($"Report # : {SiteReportDb.ReportNumber}").FontFamily("Cairo").FontSize(10);
                            row.RelativeItem().Text($"Invoice # {InvoiceNum}:").FontFamily("Cairo").FontSize(10);
                        });

                        col.Item().Row(row =>
                        {
                            row.Spacing(10); // المسافة بين العناصر
                            row.RelativeItem().Text($"Company Name : {SiteReportDb.CompanyName}").FontFamily("Cairo").FontSize(10);

                            row.Spacing(20);
                            row.RelativeItem().Text($"Site Name # : {SiteReportDb.SiteName}").FontFamily("Cairo").FontSize(10);
                        });
                        col.Item().Row(row =>
                        {
                            row.Spacing(10); // المسافة بين العناصر
                            row.RelativeItem().Text($"Project Name : {SiteReportDb.ProjectName}").FontFamily("Cairo").FontSize(10);
                            row.Spacing(20);
                            row.RelativeItem().Text($"location  : {SiteReportDb.Projectlocation}").FontFamily("Cairo").FontSize(10);
                            row.RelativeItem().Text($" Description : {SiteReportDb.ProjectDescription}").FontFamily("Cairo").FontSize(10);
                        });


                        //col.Item().Row(row =>
                        //{
                        //    row.Spacing(20); // المسافة بين العناصر
                        //    row.RelativeItem().Text($"Report : {reportDb.Notes} ").FontFamily("Cairo").FontSize(12);

                        //});
                        col.Item().Width(PageSizes.A4.Width - 40)   // عرض A4 ناقص الهوامش
    .Element(e =>
    {
        e.Scale(0.85f)   // تصغير 15% ليظهر بشكل ممتاز داخل A4
         .Table(table =>
         {
             table.ColumnsDefinition(columns =>
             {
                 columns.RelativeColumn(60);
                 columns.RelativeColumn(10);
                 columns.RelativeColumn(10);
                 columns.RelativeColumn(20);
             });

             table.Header(header =>
             {
                 header.Cell().Border(1).Background("#f0f0f0").Padding(2).AlignCenter()
                       .Text("Items").Bold();

                 header.Cell().Border(1).Background("#f0f0f0").Padding(2).AlignCenter()
                       .Text("Review").Bold();

                 header.Cell().Border(1).Background("#f0f0f0").Padding(2).AlignCenter()
                       .Text("Fault").Bold();

                 header.Cell().Border(1).Background("#f0f0f0").Padding(2).AlignCenter()
                       .Text("Corrective").Bold();
             });

             foreach (var item in checkingItems)
             {
                 table.Cell().Border(1).Padding(1).AlignCenter()
                      .Text(item.Item).FontSize(9);

                 table.Cell().Border(1).Padding(1).AlignCenter()
                      .Text(item.Review ? "✔" : " ").FontSize(10);

                 table.Cell().Border(1).Padding(1).AlignCenter()
                      .Text(item.faultFlag ? "✔" : " ").FontSize(10);

                 table.Cell().Border(1).Padding(1).AlignCenter()
                      .Text(item.CorrectiveAction ?? "-").FontSize(9);
             }
         });
    });


                        col.Item().Row(row =>
                        {
                            row.Spacing(10); // المسافة بين العناصر
                            row.RelativeItem().Text($"Team Num. : {SiteReportDb.TeamNum}").FontFamily("Cairo").FontSize(10);
                            row.Spacing(20);
                            row.RelativeItem().Text($"Leader Name  : {SiteReportDb.TeamLeaderName}").FontFamily("Cairo").FontSize(10);
                            row.RelativeItem().Text($"Leader Num. {SiteReportDb.TeamLeaderNum}").FontFamily("Cairo").FontSize(10);
                        });




                        col.Item().Width(PageSizes.A4.Width - 40)   // عرض A4 ناقص الهوامش
    .Element(e =>
    {
        e.Scale(0.85f)   // تصغير 15% ليظهر بشكل ممتاز داخل A4
         .Table(table =>
         {
             table.ColumnsDefinition(columns =>
             {
                 columns.RelativeColumn(20);
                 columns.RelativeColumn(80);

             });

             table.Header(header =>
             {
                 header.Cell().Border(1).Background("#f0f0f0").Padding(2).AlignCenter()
                       .Text("Index").Bold();

                 header.Cell().Border(1).Background("#f0f0f0").Padding(2).AlignCenter()
                       .Text("Name").Bold();

             });

             var i = 1;
             foreach (var item in members.Split(","))
             {
                 table.Cell().Border(1).Padding(1).AlignCenter()
                      .Text(i).FontSize(9);

                 table.Cell().Border(1).Padding(1).AlignCenter()
                      .Text(item).FontSize(10);

                 i++;
             }
         });
    });


                        //  col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);







                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين العناصر
                            row.RelativeItem().Text($"PhoneNum. : {SiteReportDb.PhoneNum} ").FontFamily("Cairo").FontSize(12);

                        });
                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين العناصر
                            row.RelativeItem().Text($"Notes : {SiteReportDb.Notes} ").FontFamily("Cairo").FontSize(12);

                        });

                        col.Item().Row(row =>
                        {
                            row.Spacing(50); // المسافة بين العناصر
                            row.RelativeItem().Text($"Marina REP. : {SiteReportDb.TechName} ").FontFamily("Cairo").FontSize(12);
                            //row.RelativeItem().Text($"PhoneNum. : {reportDb.PhoneNum} ").FontFamily("Cairo").FontSize(12);
                            row.RelativeItem().Text($"Site REP. : {SiteReportDb.ClientName} ").FontFamily("Cairo").FontSize(12);

                        });
                        // صورة داخل المحتوى كمثال إضافي
                        col.Item().Layers(layers =>
                        {
                            // ✅ الطبقة الأساسية (التواقيع)
                            layers.PrimaryLayer().Row(row =>
                            {
                                row.Spacing(15);

                                // الصورة الأولى (توقيع الفني)
                                row.RelativeItem().Element(e =>
                                {

                                    e.Padding(5)
             .Width(150)
             .Height(100)
             .Image(techImage)
             .FitWidth();
                                });

                                // الصورة الثانية (توقيع العميل)
                                row.RelativeItem().Element(e =>
                                {
                                    e.Padding(5)
             .Width(150)
             .Height(100)
             .Image(clientImage)
             .FitWidth();
                                });
                            });

                            // ✅ الطبقة الثانية (الختم فوق الصورتين)
                            layers.Layer()
        .AlignCenter()
        .AlignMiddle()
        .Element(e =>
        {
            e.Width(100)         // ← الحجم يوضع هنا (على الـ container)
             .Image(sealBytes)   // ← وأخيرًا الصورة
             .FitWidth();
        });
                        });



                    });


                // ===== ذيل الصفحة =====
                page.Footer()
                    .BorderBottom(1)
    .PaddingVertical(2)
    .Row(row =>
    {
        row.Spacing(10);

        // ✅ الشعار على اليسار
        row.ConstantItem(150).Image(logoBytesFooter).FitWidth();

        // ✅ معلومات الاتصال في المنتصف
        row.RelativeItem().Column(col =>
        {
            col.Spacing(6);

            col.Item().Row(r =>
            {
                r.Spacing(6);
                r.RelativeItem().Background("#B91C1C").Padding(5).Text("qatar@marinaplt.com").FontColor(Colors.White).FontFamily("Cairo").FontSize(12);
                r.RelativeItem().Background("#1E3A8A").Padding(5).Text("www.marinaplt.com").FontColor(Colors.White).FontFamily("Cairo").FontSize(12);
            });

            col.Item().Row(r =>
            {
                r.Spacing(6);
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


    [HttpGet("GetDeliveryReportPdf")]
    public IActionResult GetDeliveryReportPdf(int Id , string InvoiceNum = " ")
    {
        QuestPDF.Settings.License=LicenseType.Community;

        var fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "Cairo-Regular.ttf");
        FontManager.RegisterFont(System.IO.File.OpenRead(fontPath));

        var DeliveryReportDb =  _db.DeliveryReport
        .Include(x => x.checkingItemReport).ThenInclude(r=>r.deliveryNote)
        .FirstOrDefault(x => x.Id == Id);

        // var deliveryNoteDb = await _db.DeliveryNotes.ToListAsync();

        if(DeliveryReportDb==null)
            return NotFound();
        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        var DeliveryReport = new DeliveryReportDetailDto
        {
            CompanyName=DeliveryReportDb.CompanyName ,
            ReportNumber=DeliveryReportDb.ReportNumber ,
            Date=DeliveryReportDb.Date ,
            ClientSignaturePath=DeliveryReportDb.ClientSignaturePath!=null ? baseUrl+DeliveryReportDb.ClientSignaturePath : null ,
            TechSignaturePath=baseUrl+DeliveryReportDb.TechSignaturePath!=null ? baseUrl+DeliveryReportDb.TechSignaturePath : null ,
            checkingItems=DeliveryReportDb.checkingItemReport.Select(a => new DeliveryItemsDto
            {
                Description=a.deliveryNote.Description ,
                DeliveryType=a.deliveryNote.DeliveryType ,
                Quantity=a.Quantity ,
                Unit=a.UnitValue!=null ? a.UnitValue : null
            }).ToList()
        };









        var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Picture1.jpg");
        var logoBytes = System.IO.File.ReadAllBytes(logoPath);

        var sealPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "seal.png");
        var sealBytes = System.IO.File.ReadAllBytes(sealPath);

        var logoPathFooter = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Picture1.jpg");
        var logoBytesFooter = System.IO.File.ReadAllBytes(logoPathFooter);

        var techPath = DeliveryReportDb.TechSignaturePath?.TrimStart('/');
        var techImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", techPath);

        byte[] techImage = Array.Empty<byte>();
        if(System.IO.File.Exists(techImagePath))
        {
            techImage=System.IO.File.ReadAllBytes(techImagePath);
        }

        var clientPath = DeliveryReportDb.ClientSignaturePath?.TrimStart('/');
        var clientImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", clientPath);

        byte[] clientImage = Array.Empty<byte>();
        if(System.IO.File.Exists(clientImagePath))
        {
            clientImage=System.IO.File.ReadAllBytes(clientImagePath);
        }



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

                                e. Width(150)
                                 .Height(75)
                                .Image(logoBytes)
                                 .FitWidth()
                                 ;  // يجعل الصورة تمتد بعرض الصفحة تلقائيًا
                            });
                        col.Item().LineHorizontal(1)
    .LineColor(Colors.Grey.Lighten2);
                        // 📝 العنوان أسفل الصورة
                        col.Item()
                            .AlignCenter()
                            .PaddingTop(5)
                            .Text("Site Report")
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
                            row.RelativeItem().Text($"Date : {DeliveryReportDb.Date.ToShortDateString()}").FontFamily("Cairo").FontSize(12);
                            row.Spacing(60);
                            row.RelativeItem().Text($"Report # : {DeliveryReportDb.ReportNumber}").FontFamily("Cairo").FontSize(12);
                            row.RelativeItem().Text($"Invoice # : {InvoiceNum}").FontFamily("Cairo").FontSize(12);
                        });

                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين العناصر
                            row.RelativeItem().Text($"Company Name : {DeliveryReportDb.CompanyName}").FontFamily("Cairo").FontSize(12);
                        });



                        //col.Item().Row(row =>
                        //{
                        //    row.Spacing(20); // المسافة بين العناصر
                        //    row.RelativeItem().Text($"Report : {reportDb.Notes} ").FontFamily("Cairo").FontSize(12);

                        //});
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(60);
                                columns.RelativeColumn(20);
                                columns.RelativeColumn(10);
                                columns.RelativeColumn(10);
                            });

                            // ===== هيدر الجدول =====
                            table.Header(header =>
                            {
                                header.Cell().Border(1).Background("#f0f0f0").Padding(2).Text("Description").FontFamily("Cairo").Bold();
                                header.Cell().Border(1).Background("#f0f0f0").Padding(2).Text("DeliveryType").FontFamily("Cairo").Bold();
                                header.Cell().Border(1).Background("#f0f0f0").Padding(2).Text("Qty").FontFamily("Cairo").Bold();
                                header.Cell().Border(1).Background("#f0f0f0").Padding(2).Text("Unit").FontFamily("Cairo").Bold();
                            });
                            foreach (var item in DeliveryReport.checkingItems)
                            {
                                table.Cell().Border(1).PaddingVertical(1).PaddingHorizontal(4)
                             .Text(item.Description ).FontFamily("Cairo").FontSize(9);

                                table.Cell().Border(1).PaddingVertical(1).PaddingHorizontal(4)
                             .Text(item.DeliveryType ?? "-").FontFamily("Cairo").FontSize(9);
                                table.Cell().Border(1).PaddingVertical(1).PaddingHorizontal(4)
                             .Text(item.Quantity ).FontFamily("Cairo").FontSize(9);

                                table.Cell().Border(1).PaddingVertical(1).PaddingHorizontal(4)
                             .Text(item.Unit ).FontFamily("Cairo").FontSize(10);



                            }



                        });

                        //  col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);







                        col.Item().Row(row =>
                        {
                            row.Spacing(20); // المسافة بين العناصر
                            row.RelativeItem().Text($"PhoneNum. : {DeliveryReportDb.PhoneNum} ").FontFamily("Cairo").FontSize(12);

                        });

                        col.Item().Row(row =>
                        {
                            row.Spacing(50); // المسافة بين العناصر
                            row.RelativeItem().Text($"Marina REP. : {DeliveryReportDb.TechName} ").FontFamily("Cairo").FontSize(12);
                            //row.RelativeItem().Text($"PhoneNum. : {reportDb.PhoneNum} ").FontFamily("Cairo").FontSize(12);
                            row.RelativeItem().Text($"Site REP. : {DeliveryReportDb.ClientName} ").FontFamily("Cairo").FontSize(12);

                        });
                        // صورة داخل المحتوى كمثال إضافي
                        col.Item().Layers(layers =>
                        {
                            // ✅ الطبقة الأساسية (التواقيع)
                            layers.PrimaryLayer().Row(row =>
                            {
                                row.Spacing(15);

                                // الصورة الأولى (توقيع الفني)
                                row.RelativeItem().Element(e =>
                                {

                                    e.Padding(5)
             .Width(150)
             .Height(100)
             .Image(techImage)
             .FitWidth();
                                });

                                // الصورة الثانية (توقيع العميل)
                                row.RelativeItem().Element(e =>
                                {
                                    e.Padding(5)
             .Width(150)
             .Height(100)
             .Image(clientImage)
             .FitWidth();
                                });
                            });

                            // ✅ الطبقة الثانية (الختم فوق الصورتين)
                            layers.Layer()
        .AlignCenter()
        .AlignMiddle()
        .Element(e =>
        {
            e.Width(100)         // ← الحجم يوضع هنا (على الـ container)
             .Image(sealBytes)   // ← وأخيرًا الصورة
             .FitWidth();
        });
                        });



                    });


                // ===== ذيل الصفحة =====
                page.Footer()
                    .BorderBottom(1)
    .PaddingVertical(2)
    .Row(row =>
    {
        row.Spacing(10);

        // ✅ الشعار على اليسار
        row.ConstantItem(150).Image(logoBytesFooter).FitWidth();

        // ✅ معلومات الاتصال في المنتصف
        row.RelativeItem().Column(col =>
        {
            col.Spacing(6);

            col.Item().Row(r =>
            {
                r.Spacing(6);
                r.RelativeItem().Background("#B91C1C").Padding(5).Text("qatar@marinaplt.com").FontColor(Colors.White).FontFamily("Cairo").FontSize(12);
                r.RelativeItem().Background("#1E3A8A").Padding(5).Text("www.marinaplt.com").FontColor(Colors.White).FontFamily("Cairo").FontSize(12);
            });

            col.Item().Row(r =>
            {
                r.Spacing(6);
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





    [HttpGet("GetPagedElevatorReport")]
    public async Task<IActionResult> GetPagedElevatorReport(long userId , int pageNumber = 1 , int pageSize = 10)
    {
        if(pageNumber<1)
            pageNumber=1;
        if(pageSize<1)
            pageSize=10;

        var totalReports = await _db.Elevator.CountAsync();
        var reports = new List<Elevator>();

        if(userId>0)
        {
            reports=await _db.Elevator
                .Where(x => x.UserId==userId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber-1)*pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        else
        {
            reports=await _db.Elevator
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber-1)*pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        var imagesDb = await _db.ElevatorImage.Where(x => reports.Select(y => y.Id).Contains(x.ElevatorId)).ToListAsync();
        var baseUrl = $"{Request.Scheme}://{Request.Host}";


        var svgFolder = Path.Combine(_env.WebRootPath, "elevatorsvg");

        // إنشاء المجلد إذا لم يكن موجودًا
        if(!Directory.Exists(svgFolder))
        {
            Directory.CreateDirectory(svgFolder);
        }
        else
        {
            // تنظيف الملفات القديمة
            var oldFiles = Directory.GetFiles(svgFolder, "*.svg");
            foreach(var file in oldFiles)
                System.IO.File.Delete(file);
        }





        var pagedReports = reports.Select(x =>
        {
            var svgRequest = new SvgRequest
            {
                Width = x.resizableSquarewidth,
                Height = x.resizableSquareHeight,
                InnerWidth = x.widthShape,
                InnerHeight = x.heightShape,
                TopRadius = x.radiusShape
            };

            string svgString = "";
            if (x.shapeType == "square-semicircle")
            {
                svgString = GenerateSvgStringCorrected(svgRequest);

            }
            else if (x.shapeType == "square")
            {
                svgString = GenerateSvgString(svgRequest);

            }
            else if (x.shapeType == "circle")
            {
                svgString = GenerateSvgCircle(svgRequest);

            }

            //// اسم ملف فريد لكل تقرير
            //var svgFileName = $"elevator_{x.Id}.svg";
            //var svgFilePath = Path.Combine(svgFolder, svgFileName);

            //// حفظ الملف قبل استخدامه
            //System.IO.File.WriteAllText(svgFilePath, svgString);

            //var svgUrl = $"{Request.Scheme}://{Request.Host}/elevatorsvg/{svgFileName}";




            return new GetAllElevatorDto
            {
                Id = x.Id,
                Date = x.Date,
                ReportNumber = x.ReportNumber,
                reportType = x.reportType,
                typeElevator = x.typeElevator,
                InvoiceNumber = x.InvoiceNumber,
                CompanyName = x.CompanyName,
                ProjectAddress = x.ProjectAddress,
                resizableSquarewidth = x.resizableSquarewidth,
                resizableSquareHeight = x.resizableSquareHeight,
                shapeType = x.shapeType,
                widthShape = x.widthShape,
                heightShape = x.heightShape,
                radiusShape = x.radiusShape,
                //directionShape = x.directionShape,
                floors = x.floors,
                foundationHeight = x.foundationHeight,
                capinaHeight = x.capinaHeight,
                capinaStatus = x.capinaStatus,
                floorHeights = !string.IsNullOrEmpty(x.floorHeights)
                    ? x.floorHeights.Trim('"', '[', ']').Replace("\",\"", ",")
                    : string.Empty,
                workRequied = !string.IsNullOrEmpty(x.workRequied)
                    ? x.workRequied.Trim('"', '[', ']').Replace("\",\"", ",")
                    : string.Empty,
                Notes = x.Notes,
                CreatedAt = x.CreatedAt,
              //  ClientName = x.ClientName,
                TechName = x.TechName,
                salesName = x.salesName,
                wellStatus = x.wellStatus,
                PhoneNum = x.PhoneNum,
                //ClientSignaturePath = baseUrl + x.ClientSignaturePath,
                //TechSignaturePath = baseUrl + x.TechSignaturePath,
                Images = imagesDb
                    .Where(y => y.ElevatorId == x.Id)
                    .Select(p => baseUrl + p.FilePath)
                    .ToList(),
                imageSva = new string[] { x.WellImagePath, x.DirectionImagePath, x.ResizableImagePath }
            .Where(u => !string.IsNullOrEmpty(u))
            .Select(u => baseUrl + u)
            .ToArray(),

                doorDirections =  !string.IsNullOrEmpty(x.doorDirections)
                    ? x.doorDirections.Trim('"', '[', ']').Replace("\",\"", ",")
                    : string.Empty,

            };
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




    [HttpGet("GetElevatorReportPdf")]
    public async Task<IActionResult> GetElevatorReportPdf(int Id , string InvoiceNum = " ")
    {
        QuestPDF.Settings.License=LicenseType.Community;

        var fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "Cairo-Regular.ttf");
        FontManager.RegisterFont(System.IO.File.OpenRead(fontPath));

        var ElevatorReportDb =  _db.Elevator .FirstOrDefault(x => x.Id == Id);

        // var deliveryNoteDb = await _db.DeliveryNotes.ToListAsync();

        if(ElevatorReportDb==null)
            return NotFound();
        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        var svgFolder = Path.Combine(_env.WebRootPath, "elevatorsvg");
        if(!Directory.Exists(svgFolder))
        {
            Directory.CreateDirectory(svgFolder);
        }
        else
        {
            // تنظيف الملفات القديمة
            var oldFiles = Directory.GetFiles(svgFolder, "*.svg");
            foreach(var file in oldFiles)
                System.IO.File.Delete(file);
        }


        var svgRequest = new SvgRequest
        {
            Width = ElevatorReportDb.resizableSquarewidth,
            Height = ElevatorReportDb.resizableSquareHeight,
            InnerWidth = ElevatorReportDb.widthShape,
            InnerHeight = ElevatorReportDb.heightShape,
            TopRadius = ElevatorReportDb.radiusShape
        };

        string svgString = "";
        if(ElevatorReportDb.shapeType=="square-semicircle")
        {
            svgString=GenerateSvgStringCorrected(svgRequest);

        }
        else if(ElevatorReportDb.shapeType=="square")
        {
            svgString=GenerateSvgString(svgRequest);

        }
        else if(ElevatorReportDb.shapeType=="circle")
        {
            svgString=GenerateSvgCircle(svgRequest);

        }







        var DeliveryReport = new GetAllElevatorDto
        {
            CompanyName=ElevatorReportDb.CompanyName ,
            ReportNumber=ElevatorReportDb.ReportNumber ,
            Date=ElevatorReportDb.Date ,
          //  ClientSignaturePath=ElevatorReportDb.ClientSignaturePath!=null ? baseUrl+ElevatorReportDb.ClientSignaturePath : null ,
           // TechSignaturePath=baseUrl+ElevatorReportDb.TechSignaturePath!=null ? baseUrl+ElevatorReportDb.TechSignaturePath : null ,
            typeElevator = ElevatorReportDb.typeElevator,
            shapeType = ElevatorReportDb.shapeType ,
            resizableSquarewidth = ElevatorReportDb.resizableSquarewidth ,
            resizableSquareHeight = ElevatorReportDb.resizableSquareHeight,
            widthShape = ElevatorReportDb.widthShape ,
            heightShape = ElevatorReportDb.heightShape ,
            radiusShape =ElevatorReportDb.radiusShape,
           // directionShape = ElevatorReportDb.directionShape ,
            foundationHeight = ElevatorReportDb.foundationHeight,
            floorHeights = !string.IsNullOrEmpty(ElevatorReportDb.floorHeights)
 ? ElevatorReportDb.floorHeights.Trim('"', '[', ']').Replace("\",\"", ",")
 : string.Empty,
            workRequied = !string.IsNullOrEmpty(ElevatorReportDb.workRequied)
    ? ElevatorReportDb.workRequied
        .Replace("[", "")
        .Replace("]", "")
        .Replace("\"", "")
        .Replace(",", ", ")
    : string.Empty,

            doorDirections =  !string.IsNullOrEmpty(ElevatorReportDb.doorDirections)
 ? ElevatorReportDb.doorDirections.Trim('"', '[', ']').Replace("\",\"", ",")
 : string.Empty,


        };









        var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Picture1.jpg");
        var logoBytes = System.IO.File.ReadAllBytes(logoPath);

        var sealPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "seal.png");
        var sealBytes = System.IO.File.ReadAllBytes(sealPath);

        var logoPathFooter = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Picture1.jpg");
        var logoBytesFooter = System.IO.File.ReadAllBytes(logoPathFooter);

        var techPath = ElevatorReportDb.WellImagePath?.TrimStart('/');

        var wellImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", techPath);

        byte[] wellImage = Array.Empty<byte>();
        if(System.IO.File.Exists(wellImagePath))
        {
            wellImage=System.IO.File.ReadAllBytes(wellImagePath);
        }
        var clientPath = ElevatorReportDb.DirectionImagePath?.TrimStart('/');

        var DirectionImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", clientPath);

        byte[] DirectionImage = Array.Empty<byte>();
        if(System.IO.File.Exists(DirectionImagePath))
        {
            DirectionImage=System.IO.File.ReadAllBytes(DirectionImagePath);
        }


        var ResizPath = ElevatorReportDb.ResizableImagePath?.TrimStart('/');

        var ResizImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", ResizPath);

        byte[] ResizImage = Array.Empty<byte>();
        if(System.IO.File.Exists(ResizImagePath))
        {
            ResizImage=System.IO.File.ReadAllBytes(ResizImagePath);
        }










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
                    col.Item().AlignCenter().Element(e =>
                    {
                        e.Width(100).Height(50).Image(logoBytes).FitWidth();
                    });

                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    // عنوان التقرير
                    col.Item().AlignCenter().PaddingTop(5)
                .Text($"{ElevatorReportDb.reportType}")
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
                        row.RelativeItem()
                    .Text($"التاريخ: {ElevatorReportDb.Date.ToShortDateString()}")
                    .FontFamily("Cairo").FontSize(12);

                        row.RelativeItem()
                    .Text($"رقم التقرير: {ElevatorReportDb.ReportNumber}")
                    .FontFamily("Cairo").FontSize(12);

                        row.RelativeItem()
                    .Text($"رقم الفاتورة: {InvoiceNum}")
                    .FontFamily("Cairo").FontSize(12);
                    });

                    // Row 2: اسم الشركة
                    col.Item().AlignRight()
                .Text($"اسم الشركة أو العميل: {ElevatorReportDb.CompanyName}")
                .FontFamily("Cairo").FontSize(12);
                    col.Item().AlignRight().Text($"رقم الهاتف: {ElevatorReportDb.PhoneNum}").FontFamily("Cairo").FontSize(12);

                    // Row 3: مقاسات البئر والاتجاه
                    col.Item().AlignRight().Row(row =>
                    {
                        // تجميع المقاسات
                        var parts = new List<string>();
                        if (ElevatorReportDb.widthShape > 0)
                            parts.Add($"العرض {ElevatorReportDb.widthShape}");
                        if (ElevatorReportDb.heightShape > 0)
                            parts.Add($"العمق {ElevatorReportDb.heightShape}");
                        if (ElevatorReportDb.radiusShape > 0)
                            parts.Add($"نصف القطر {ElevatorReportDb.radiusShape}");
                        string finalText = "مقاسات البئر: " + string.Join("   ", parts);

                        row.RelativeItem()
                    .Text(finalText)
                    .FontFamily("Cairo")
                    .FontSize(12);

                    //    // السهم
                    //    string directionSymbol = ElevatorReportDb.directionShape switch
                    //    {
                    //        9 => "←",
                    //        3 => "→",
                    //        12 => "↑",
                    //        6 => "↓",
                    //        _ => ""
                    //    };

                    //    row.ConstantItem(80)
                    //.Text($"{directionSymbol} {ElevatorReportDb.directionShape}")
                    //.FontFamily("Cairo")
                    //.FontSize(15);
                    });

                    // Row 4: مقاسات سيبس
                    col.Item().AlignRight()
                .Text($"مقاسات سيبس — العرض {ElevatorReportDb.resizableSquarewidth}    العمق {ElevatorReportDb.resizableSquareHeight}")
                .FontFamily("Cairo").FontSize(12);

                    // Row 5: مقاسات فتحة الباب
                    col.Item().AlignRight()
                .Text($"مقاسات فتحة الباب — العرض {ElevatorReportDb.directionWidth}   الطول {ElevatorReportDb.directionHeight}")
                .FontFamily("Cairo").FontSize(12);

                    // Row 6: ثلاث صور في نفس الصف
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().AlignCenter().Element(e =>
                        {
                            e.Width(150).Height(150).Image(wellImage);
                        });

                        row.RelativeItem().AlignCenter().Element(e =>
                        {
                            e.Width(150).Height(150).Image(DirectionImage);
                        });

                        row.RelativeItem().AlignCenter().Element(e =>
                        {
                            e.Width(150).Height(150).Image(ResizImagePath);
                        });
                    });

                    // Row 7: نوع المصعد
                    col.Item().AlignRight()
                .Text($"نوع المصعد : {ElevatorReportDb.typeElevator}")
                .FontFamily("Cairo").FontSize(12);

                    // ===== جدول الأدوار =====
                    col.Item().AlignRight().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(50);
                            columns.RelativeColumn(25);
                            columns.RelativeColumn(25);
                        });

                        // Header
                        table.Header(header =>
                        {
                            header.Cell().Border(1).Padding(2).AlignCenter().Text("الأدوار").FontFamily("Cairo").Bold();
                            header.Cell().Border(1).Padding(2).AlignCenter().Text("الارتفاع").FontFamily("Cairo").Bold();
                            header.Cell().Border(1).Padding(2).AlignCenter().Text("الاتجاه").FontFamily("Cairo").Bold();
                        });

                        // تحويل القوائم
                        var heightList = ElevatorReportDb.floorHeights
                    .Replace("[", "").Replace("]", "").Replace("\"", "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse).ToList();

                        var directionList = ElevatorReportDb.doorDirections
                    .Replace("[", "").Replace("]", "").Replace("\"", "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse).ToList();

                        // الأرضي
                        table.Cell().Border(1).Padding(4).AlignCenter().Text("الكابينه").FontFamily("Cairo").FontSize(9);
                        if(ElevatorReportDb.capinaHeight != 0)
                        { table.Cell().Border(1).Padding(4) .AlignCenter() .Text(ElevatorReportDb.capinaHeight).FontFamily("Cairo").FontSize(9); }
                        else
                        { table.Cell().Border(1).Padding(4) .AlignCenter() .Text(ElevatorReportDb.capinaStatus).FontFamily("Cairo").FontSize(9); }
                        table.Cell().Border(1).Padding(4) .AlignCenter() .Text("  ").FontFamily("Cairo").FontSize(9);
                        // باقي الأدوار (عكسي)
                        for (int i = heightList.Count - 1; i >= 1; i--)
                        {
                            table.Cell().Border(1).Padding(4).AlignCenter().Text($"الدور  {i}").FontFamily("Cairo").FontSize(9);
                            if(heightList[i] ==1001)
                            {
                                table.Cell().Border(1).Padding(4).AlignCenter().Text("تحت الانشاء").FontFamily("Cairo").FontSize(9);

                            }
                            else
                            {
                                table.Cell().Border(1).Padding(4).AlignCenter().Text(heightList[i]).FontFamily("Cairo").FontSize(9);

                            }
                            if(directionList[i] ==66)
                            {
                                table.Cell().Border(1).Padding(4).AlignCenter().Text("تحت الانشاء").FontFamily("Cairo").FontSize(9);

                            }
                            else
                            {
                                table.Cell().Border(1).Padding(4).AlignCenter().Text(directionList[i]).FontFamily("Cairo").FontSize(9);
                            }
                        }



                           // الحراج
                        table.Cell().Border(1).Padding(4).AlignCenter().Text("الجراج").FontFamily("Cairo").FontSize(9);
                        table.Cell().Border(1).Padding(4).AlignCenter().Text(heightList[0]).FontFamily("Cairo").FontSize(9);
                        table.Cell().Border(1).Padding(4).AlignCenter().Text(directionList[0]).FontFamily("Cairo").FontSize(9);


                        table.Cell().Border(1).Padding(4) .AlignCenter() .Text("حفره البئر").FontFamily("Cairo").FontSize(9);
                        if(ElevatorReportDb.foundationHeight != 0)
                        { table.Cell().Border(1).Padding(4) .AlignCenter() .Text(ElevatorReportDb.foundationHeight).FontFamily("Cairo").FontSize(9); }
                        else
                        { table.Cell().Border(1).Padding(4) .AlignCenter() .Text(ElevatorReportDb.wellStatus).FontFamily("Cairo").FontSize(9); }
                        table.Cell().Border(1).Padding(4) .AlignCenter() .Text("  ").FontFamily("Cairo").FontSize(9);

                    });
                    col.Spacing(5);

                    col.Item().AlignRight().Text($" الاعمال المطلوبة: {DeliveryReport.workRequied}").FontFamily("Cairo").FontSize(12);

                    // ملاحظات
                    col.Item().AlignRight().Text($"ملاحظات: {ElevatorReportDb.Notes}").FontFamily("Cairo").FontSize(12);

                    // بيانات الاتصال
                    
                    col.Item().AlignLeft().PaddingLeft(20).Text($"الفني المسئول: {ElevatorReportDb.TechName}").FontFamily("Cairo").FontSize(12);
                    col.Item().AlignLeft().PaddingLeft(20).Text($"مسئول المبيعات: {ElevatorReportDb.salesName}").FontFamily("Cairo").FontSize(12);
                });

                // ===== Footer =====
                page.Footer().Row(row =>
                {
                    row.ConstantItem(150).Image(logoBytesFooter).FitWidth();

                    row.RelativeItem().Column(col =>
                    {
                        col.Spacing(5);

                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Background("#B91C1C").Padding(5).Text("qatar@marinaplt.com").FontColor(Colors.White).FontFamily("Cairo").FontSize(12);
                            r.RelativeItem().Background("#1E3A8A").Padding(5).Text("www.marinaplt.com").FontColor(Colors.White).FontFamily("Cairo").FontSize(12);
                        });

                        col.Item().Row(r =>
                        {
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

    private string GenerateSvgStringCorrected(SvgRequest dto)
    {
        int w = dto.Width;
        int h = dto.Height;
        int iw = (int)dto.InnerWidth;
        int ih = (int)dto.InnerHeight;
        double radius = (double)dto.TopRadius;

        // مساحة إضافية أعلى لنصف الدائرة وخطوط الأبعاد
        int topMargin = (int)Math.Ceiling(radius + 50);

        // مركز المربع الداخلي مع مراعاة topMargin
        int innerX = (w - iw) / 2;
        int innerY = topMargin + (h - ih - topMargin) / 2;

        return $@"
<svg xmlns=""http://www.w3.org/2000/svg"" width=""{w+200}"" height=""{h+120}"" viewBox=""{-60} {-60} {w+120} {h+120}"">

    <!-- ======================= FRAME ======================= -->
    <rect x=""0"" y=""0"" width=""{w}"" height=""{h}""
          fill=""white"" stroke=""black"" stroke-width=""5"" />

    <!-- ======================= INNER SHAPE: Square + Top Half Circle ======================= -->
    <rect x=""{innerX}"" y=""{innerY}"" width=""{iw}"" height=""{ih}""
          fill=""none"" stroke=""red"" stroke-width=""3"" />

    <path d=""M {innerX} {innerY} A {radius} {radius} 0 0 1 {innerX+iw} {innerY}""
          fill=""none"" stroke=""red"" stroke-width=""3"" />

    <!-- ======================= DIMENSIONS: INNER ======================= -->

    <!-- Inner Width -->
    <line x1=""{innerX}"" y1=""{innerY-40}"" x2=""{innerX+iw}"" y2=""{innerY-40}"" stroke=""red"" stroke-width=""2"" />
    <polyline points=""{innerX},{innerY-40} {innerX+15},{innerY-45} {innerX+15},{innerY-35}"" fill=""red"" />
    <polyline points=""{innerX+iw},{innerY-40} {innerX+iw-15},{innerY-45} {innerX+iw-15},{innerY-35}"" fill=""red"" />
    <text x=""{innerX+iw/2}"" y=""{innerY-50}"" font-size=""22"" text-anchor=""middle"" fill=""red"">
        {iw} mm
    </text>

    <!-- Inner Height -->
    <line x1=""{innerX+iw+40}"" y1=""{innerY}"" x2=""{innerX+iw+40}"" y2=""{innerY+ih}"" stroke=""red"" stroke-width=""2"" />
    <polyline points=""{innerX+iw+40},{innerY} {innerX+iw+35},{innerY+15} {innerX+iw+45},{innerY+15}"" fill=""red"" />
    <polyline points=""{innerX+iw+40},{innerY+ih} {innerX+iw+35},{innerY+ih-15} {innerX+iw+45},{innerY+ih-15}"" fill=""red"" />
    <text x=""{innerX+iw+55}"" y=""{innerY+ih/2}"" font-size=""22"" text-anchor=""start"" dominant-baseline=""middle"" fill=""red"" transform=""rotate(90,{innerX+iw+55},{innerY+ih/2})"">
        {ih} mm
    </text>

    <!-- Half Circle Height -->
    <line x1=""{innerX-40}"" y1=""{innerY}"" x2=""{innerX-40}"" y2=""{innerY-radius}"" stroke=""red"" stroke-width=""2"" />
    <polyline points=""{innerX-40},{innerY} {innerX-45},{innerY-15} {innerX-35},{innerY-15}"" fill=""red"" />
    <polyline points=""{innerX-40},{innerY-radius} {innerX-45},{innerY-radius+15} {innerX-35},{innerY-radius+15}"" fill=""red"" />
    <text x=""{innerX-55}"" y=""{innerY-radius/2}"" font-size=""22"" text-anchor=""middle"" fill=""red"">
        {radius} mm
    </text>

    <!-- ======================= DIMENSIONS: OUTER ======================= -->

    <!-- Horizontal (Outer Width) -->
    <line x1=""0"" y1=""{h+40}"" x2=""{w}"" y2=""{h+40}"" stroke=""black"" stroke-width=""2"" />
    <polyline points=""0,{h+40} 15,{h+35} 15,{h+45}"" fill=""black"" />
    <polyline points=""{w},{h+40} {w-15},{h+35} {w-15},{h+45}"" fill=""black"" />
    <text x=""{w/2}"" y=""{h+30}"" font-size=""26"" text-anchor=""middle"">{w} mm</text>

    <!-- Vertical (Outer Height) -->
    <line x1=""{w+40}"" y1=""0"" x2=""{w+40}"" y2=""{h}"" stroke=""black"" stroke-width=""2"" />
    <polyline points=""{w+40},0 {w+35},15 {w+45},15"" fill=""black"" />
    <polyline points=""{w+40},{h} {w+35},{h-15} {w+45},{h-15}"" fill=""black"" />
    <text x=""{w+50}"" y=""{h/2}"" font-size=""26"" text-anchor=""start"" dominant-baseline=""middle"" transform=""rotate(90,{w+50},{h/2})"">{h} mm</text>

</svg>";
    }
    private string GenerateSvgString(SvgRequest dto)
    {
        int w = dto.Width;
        int h = dto.Height;
        int iw = (int)dto.InnerWidth;
        int ih = (int)dto.InnerHeight;

        int margin = 80; // مساحة كافية للأبعاد

        int svgWidth = w + margin * 2;
        int svgHeight = h + margin * 2;

        // center the main box including margin
        int offsetX = margin;
        int offsetY = margin;

        int innerX = offsetX + (w - iw) / 2;
        int innerY = offsetY + (h - ih) / 2;

        return $@"
<svg xmlns=""http://www.w3.org/2000/svg"" 
     width=""{svgWidth}"" height=""{svgHeight}"" 
     viewBox=""0 0 {svgWidth} {svgHeight}"">

    <!-- ======================= FRAME ======================= -->
    <rect x=""{offsetX}"" y=""{offsetY}"" width=""{w}"" height=""{h}""
          fill=""white"" stroke=""black"" stroke-width=""5"" />

    <!-- ======================= INNER BOX ======================= -->
    <rect x=""{innerX}"" y=""{innerY}"" width=""{iw}"" height=""{ih}""
          fill=""none"" stroke=""red"" stroke-width=""3"" />

    <!-- ======================= OUTER DIMENSIONS ======================= -->

    <!-- Horizontal (Outer Width) -->
    <line x1=""{offsetX}"" y1=""{offsetY+h+40}"" 
          x2=""{offsetX+w}"" y2=""{offsetY+h+40}""
          stroke=""black"" stroke-width=""2"" />

    <polyline points=""{offsetX},{offsetY+h+40} {offsetX+15},{offsetY+h+35} {offsetX+15},{offsetY+h+45}"" fill=""black"" />
    <polyline points=""{offsetX+w},{offsetY+h+40} {offsetX+w-15},{offsetY+h+35} {offsetX+w-15},{offsetY+h+45}"" fill=""black"" />

    <text x=""{offsetX+w/2}"" y=""{offsetY+h+30}"" 
          font-size=""26"" text-anchor=""middle"">
        {w} mm
    </text>

    <!-- Vertical (Outer Height) -->
    <line x1=""{offsetX+w+40}"" y1=""{offsetY}"" 
          x2=""{offsetX+w+40}"" y2=""{offsetY+h}""
          stroke=""black"" stroke-width=""2"" />

    <polyline points=""{offsetX+w+40},{offsetY} {offsetX+w+35},{offsetY+15} {offsetX+w+45},{offsetY+15}"" fill=""black"" />
    <polyline points=""{offsetX+w+40},{offsetY+h} {offsetX+w+35},{offsetY+h-15} {offsetX+w+45},{offsetY+h-15}"" fill=""black"" />

    <text x=""{offsetX+w+55}"" y=""{offsetY+h/2}"" 
          font-size=""26"" text-anchor=""start"" dominant-baseline=""middle""
          transform=""rotate(90,{offsetX+w+55},{offsetY+h/2})"">
        {h} mm
    </text>


    <!-- ======================= INNER DIMENSIONS ======================= -->

    <!-- Horizontal (Inner Width) -->
    <line x1=""{innerX}"" y1=""{innerY-30}"" 
          x2=""{innerX+iw}"" y2=""{innerY-30}""
          stroke=""red"" stroke-width=""2"" />

    <polyline points=""{innerX},{innerY-30} {innerX+15},{innerY-35} {innerX+15},{innerY-25}"" fill=""red"" />
    <polyline points=""{innerX+iw},{innerY-30} {innerX+iw-15},{innerY-35} {innerX+iw-15},{innerY-25}"" fill=""red"" />

    <text x=""{innerX+iw/2}"" y=""{innerY-40}"" 
          font-size=""22"" text-anchor=""middle"" fill=""red"">
        {iw} mm
    </text>

    <!-- Vertical (Inner Height) -->
    <line x1=""{innerX+iw+30}"" y1=""{innerY}"" 
          x2=""{innerX+iw+30}"" y2=""{innerY+ih}""
          stroke=""red"" stroke-width=""2"" />

    <polyline points=""{innerX+iw+30},{innerY} {innerX+iw+25},{innerY+15} {innerX+iw+35},{innerY+15}"" fill=""red"" />
    <polyline points=""{innerX+iw+30},{innerY+ih} {innerX+iw+25},{innerY+ih-15} {innerX+iw+35},{innerY+ih-15}"" fill=""red"" />

    <text x=""{innerX+iw+45}"" y=""{innerY+ih/2}"" 
          font-size=""22"" fill=""red"" 
          text-anchor=""start"" dominant-baseline=""middle""
          transform=""rotate(90,{innerX+iw+45},{innerY+ih/2})"">
        {ih} mm
    </text>

</svg>";
    }
    private string GenerateSvgCircle(SvgRequest dto)
    {
        int w = dto.Width;
        int h = dto.Height;
        int margin = 80; // مساحة كافية للأبعاد

        int svgWidth = w + margin * 2;
        int svgHeight = h + margin * 2;

        // center the main box including margin
        int offsetX = margin;
        int offsetY = margin;

        // مركز الدائرة
        int cx = offsetX + w / 2;
        int cy = offsetY + h / 2;
        double r =(double) dto.TopRadius; // نصف القطر للدائرة

        return $@"
<svg xmlns=""http://www.w3.org/2000/svg"" 
     width=""{svgWidth}"" height=""{svgHeight}"" 
     viewBox=""0 0 {svgWidth} {svgHeight}"">

    <!-- ======================= FRAME ======================= -->
    <rect x=""{offsetX}"" y=""{offsetY}"" width=""{w}"" height=""{h}""
          fill=""white"" stroke=""black"" stroke-width=""5"" />

    <!-- ======================= INNER SHAPE: Circle ======================= -->
    <circle cx=""{cx}"" cy=""{cy}"" r=""{r}""
            fill=""none"" stroke=""red"" stroke-width=""3"" />

    <!-- ======================= OUTER DIMENSIONS ======================= -->
    <line x1=""{offsetX}"" y1=""{offsetY+h+40}"" 
          x2=""{offsetX+w}"" y2=""{offsetY+h+40}""
          stroke=""black"" stroke-width=""2"" />

    <polyline points=""{offsetX},{offsetY+h+40} {offsetX+15},{offsetY+h+35} {offsetX+15},{offsetY+h+45}"" fill=""black"" />
    <polyline points=""{offsetX+w},{offsetY+h+40} {offsetX+w-15},{offsetY+h+35} {offsetX+w-15},{offsetY+h+45}"" fill=""black"" />

    <text x=""{offsetX+w/2}"" y=""{offsetY+h+30}"" 
          font-size=""26"" text-anchor=""middle"">
        {w} mm
    </text>

    <line x1=""{offsetX+w+40}"" y1=""{offsetY}"" 
          x2=""{offsetX+w+40}"" y2=""{offsetY+h}""
          stroke=""black"" stroke-width=""2"" />

    <polyline points=""{offsetX+w+40},{offsetY} {offsetX+w+35},{offsetY+15} {offsetX+w+45},{offsetY+15}"" fill=""black"" />
    <polyline points=""{offsetX+w+40},{offsetY+h} {offsetX+w+35},{offsetY+h-15} {offsetX+w+45},{offsetY+h-15}"" fill=""black"" />

    <text x=""{offsetX+w+55}"" y=""{offsetY+h/2}"" 
          font-size=""26"" text-anchor=""start"" dominant-baseline=""middle"" 
          transform=""rotate(90,{offsetX+w+55},{offsetY+h/2})"">
        {h} mm
    </text>

    <!-- ======================= INNER DIMENSION: Radius ======================= -->
    <line x1=""{cx-r-40}"" y1=""{cy}"" x2=""{cx+r+40}"" y2=""{cy}""
          stroke=""red"" stroke-width=""2"" />

    <text x=""{cx}"" y=""{cy-r-20}"" font-size=""22"" text-anchor=""middle"" fill=""red"">
        r = {r} mm
    </text>

</svg>";
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
    private string RTL(string input)
    {
        if(string.IsNullOrWhiteSpace(input))
            return input;

        return string.Concat(input.Reverse());
    }
}



