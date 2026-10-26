namespace maria.Model
{
    // تقرير مواصفات المصعد: نفس بيانات معاينة المصعد (Elevator) + المواصفات الفنية والديكورات والطلبات الإضافية
    public class ElevatorSpecification
    {
        public int Id { get; set; }
        public int? ContractId { get; set; }
        public DateTime Date { get; set; }
        public long UserId { get; set; } = 1;
        public string? ReportNumber { get; set; }
        public string reportType { get; set; }
        public string? InvoiceNumber { get; set; }
        public string CompanyName { get; set; }
        public string salesName { get; set; }
        public string? ProjectAddress { get; set; }

        public int resizableSquarewidth { get; set; }
        public int resizableSquareHeight { get; set; }


        public string typeElevator { get; set; }
        public string shapeType { get; set; }
        public int? widthShape { get; set; }
        public int? heightShape { get; set; }
        public int? radiusShape { get; set; }
        public int floors { get; set; }
        public int garagsNum { get; set; }
        public int foundationHeight { get; set; }
        public int? capinaHeight { get; set; }
        public int? liftWidth { get; set; }
        public int? rightWidth { get; set; }
        public int? centerWidth { get; set; }
        public int directionWidth { get; set; }
        public int directionHeight { get; set; }
        public string wellStatus { get; set; }
        public string capinaStatus { get; set; }
        public string floorHeights { get; set; }
        public string? garagsHeights { get; set; }
        public string doorDirections { get; set; }
        public string? garagsDirections { get; set; }
        public string? workRequied { get; set; }

        public string? doortwoDirections { get; set; }
        public string? garagstwoDirections { get; set; }
        public bool twoDirectionFlag { get; set; }

        public string? Notes { get; set; }
        public string? WallStatusForList { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string TechName { get; set; }
        public string? PhoneNum { get; set; }


        public string? WellImagePath { get; set; }
        public string? DirectionImagePath { get; set; }
        public string? ResizableImagePath { get; set; }

        // ===== المواصفات الفنية =====
        public int? stopsCount { get; set; }          // عدد الوقفات
        public int? load { get; set; }                // الحمولة (كجم)
        public string? machineType { get; set; }      // نوع الماكينة
        public string? countryOfOrigin { get; set; }  // بلد المنشأ
        public string? speed { get; set; }            // السرعة (م/ث)

        // ===== الديكورات =====
        public string? doorType { get; set; }         // نوع الابواب / اتجاه الحركة
        public string? doorOpenSize { get; set; }     // مقاس الباب مفتوح
        public string? doorsDecor { get; set; }       // ديكور الابواب
        public string? doorFrameDecor { get; set; }   // ديكور فريم الابواب
        public string? elevatorCategory { get; set; } // فئة المصعد
        public string? stainlessType { get; set; }    // نوع الستانليس
        public string? sideWalls { get; set; }        // الجدران الجانبية
        public string? backWall { get; set; }         // الجدار الخلفي
        public string? ceiling { get; set; }          // السقف
        public string? lightingColor { get; set; }    // لون الاضاءة
        public string? flooring { get; set; }         // الارضية
        public string? innerButtons { get; set; }     // الازرار الداخلية
        public string? callButtons { get; set; }      // ازرار الاستدعاء
        public string? handrail { get; set; }         // هاندريل

        // ===== طلبات اضافية =====
        public bool dvdScreen { get; set; }           // شاشة DVD
        public bool doorSafetyKey { get; set; }       // مفتاح امان على الابواب
        public bool cabinPassword { get; set; }       // باسورد داخل الكابين
        public bool mobileLine { get; set; }          // خط جوال
        public string? control { get; set; }          // الكنترول


        public ICollection<ElevatorSpecificationImage> elevatorSpecificationFiles { get; set; } = new List<ElevatorSpecificationImage>();
    }
}
