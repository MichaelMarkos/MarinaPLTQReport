using maria.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// إعادة توليد صور رسومات المصعد (البئر / فتحة الباب / المساحة) للتقارير القديمة
// Postman:
//   POST /api/EquipmentReport/UpdateElevatorImages?reportId=1086        ← تجربة تقرير واحد
//   POST /api/EquipmentReport/UpdateElevatorImages?dryRun=true          ← يعدّ فقط بدون حفظ
//   POST /api/EquipmentReport/UpdateElevatorImages                      ← كل التقارير
//   POST /api/EquipmentReport/UpdateElevatorImages?fromId=1&toId=500    ← على دفعات
public partial class EquipmentReportController
{
    [HttpPost("UpdateElevatorImages")]
    public async Task<IActionResult> UpdateElevatorImages(
        [FromQuery] int? reportId,
        [FromQuery] int fromId = 0,
        [FromQuery] int toId = int.MaxValue,
        [FromQuery] bool dryRun = false)
    {
        string uploadRoot = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
            "elevator");
        if(!Directory.Exists(uploadRoot))
            Directory.CreateDirectory(uploadRoot);

        var query = _db.Elevator.AsQueryable();
        query=reportId.HasValue
            ? query.Where(x => x.Id==reportId.Value)
            : query.Where(x => x.Id>=fromId&&x.Id<=toId);

        var ids = await query.OrderBy(x => x.Id).Select(x => x.Id).ToListAsync();
        if(dryRun)
            return Ok(new { count = ids.Count , ids });

        int updated = 0, deleted = 0;
        var failed = new List<object>();

        foreach(var chunk in ids.Chunk(50))
        {
            var reports = await _db.Elevator.Where(x => chunk.Contains(x.Id)).ToListAsync();
            var oldFiles = new List<string>();

            foreach(var r in reports)
            {
                try
                {
                    var well = ElevatorShapeRenderer.Well(r.shapeType , r.widthShape , r.heightShape , r.radiusShape ,
                        r.liftWidth , r.rightWidth , r.centerWidth);
                    var direction = ElevatorShapeRenderer.Direction(r.directionWidth , r.directionHeight);
                    var resizable = ElevatorShapeRenderer.Resizable(r.resizableSquarewidth , r.resizableSquareHeight);

                    // اسم ملف جديد حتى لا تظهر النسخة القديمة من الكاش
                    var newWell = await SaveElevatorImage(uploadRoot , well , "well.png");
                    var newDirection = await SaveElevatorImage(uploadRoot , direction , "direction.png");
                    var newResizable = await SaveElevatorImage(uploadRoot , resizable , "resizable.png");

                    // لا نلمس التقرير إلا بعد نجاح حفظ الصور الثلاث
                    oldFiles.AddRange(new[] { r.WellImagePath , r.DirectionImagePath , r.ResizableImagePath }
                        .Select(p => OldElevatorImageFile(uploadRoot , p))
                        .OfType<string>());
                    r.WellImagePath=newWell;
                    r.DirectionImagePath=newDirection;
                    r.ResizableImagePath=newResizable;
                    updated++;
                }
                catch(Exception ex)
                {
                    failed.Add(new { r.Id , error = ex.Message });
                }
            }

            await _db.SaveChangesAsync();

            // حذف الصور القديمة فقط بعد نجاح الحفظ في قاعدة البيانات
            foreach(var file in oldFiles)
            {
                try
                {
                    if(System.IO.File.Exists(file))
                    {
                        System.IO.File.Delete(file);
                        deleted++;
                    }
                }
                catch { /* ملف مستخدم أو بدون صلاحية — نتجاهله */ }
            }
        }

        return Ok(new { total = ids.Count , updated , deleted , failedCount = failed.Count , failed });
    }

    // يحوّل المسار المخزن (/elevator/x.png أو رابط كامل) إلى مسار الملف داخل مجلد elevator فقط
    private static string? OldElevatorImageFile(string uploadRoot , string? storedPath)
    {
        if(string.IsNullOrWhiteSpace(storedPath))
            return null;

        string name = Uri.UnescapeDataString(storedPath [(storedPath.LastIndexOf('/')+1)..]);
        if(name.Length==0||name!=Path.GetFileName(name))
            return null;

        return Path.Combine(uploadRoot , name);
    }

    private static async Task<string> SaveElevatorImage(string uploadRoot , byte[] png , string suffix)
    {
        string fileName = $"{Guid.NewGuid()}_{suffix}";
        await System.IO.File.WriteAllBytesAsync(Path.Combine(uploadRoot , fileName) , png);
        return $"/elevator/{fileName}";
    }
}
