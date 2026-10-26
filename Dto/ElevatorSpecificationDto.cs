namespace maria.Dto
{
    // نفس شكل GetAllElevatorDto + بيانات تقرير مواصفات المصعد
    public class ElevatorSpecificationDto : GetAllElevatorDto
    {
        public long UserId { get; set; }
        public int? ContractId { get; set; }
        public string? ContractDetails { get; set; }

        public string? WellImagePath { get; set; }
        public string? DirectionImagePath { get; set; }
        public string? ResizableImagePath { get; set; }

        // ===== المواصفات الفنية =====
        public int? stopsCount { get; set; }
        public int? load { get; set; }
        public string? machineType { get; set; }
        public string? countryOfOrigin { get; set; }
        public string? speed { get; set; }

        // ===== الديكورات =====
        public string? doorType { get; set; }
        public string? doorOpenSize { get; set; }
        public string? doorsDecor { get; set; }
        public string? doorFrameDecor { get; set; }
        public string? elevatorCategory { get; set; }
        public string? stainlessType { get; set; }
        public string? sideWalls { get; set; }
        public string? backWall { get; set; }
        public string? ceiling { get; set; }
        public string? lightingColor { get; set; }
        public string? flooring { get; set; }
        public string? innerButtons { get; set; }
        public string? callButtons { get; set; }
        public string? handrail { get; set; }

        // ===== طلبات اضافية =====
        public bool dvdScreen { get; set; }
        public bool doorSafetyKey { get; set; }
        public bool cabinPassword { get; set; }
        public bool mobileLine { get; set; }
        public string? control { get; set; }
    }
}
