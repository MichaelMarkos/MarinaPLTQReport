namespace maria.Dto
{
    // البند القادم من الفرونت في add / edit
    // Working = true (تعمل) ، false (لا تعمل) ، null (لم يتم الفحص)
    public class PeriodicMaintanceItemDto
    {
        public int CheckingItemId { get; set; }
        public bool? Working { get; set; }
    }

    // قطعة غيار : IsSupplied = false (مطلوب) ، true (تم التوريد)
    public class PeriodicMaintanceSparePartDto
    {
        public string Name { get; set; }
        public int? Quantity { get; set; }
        public bool IsSupplied { get; set; }
    }

    public class PeriodicMaintanceReportDto
    {
        public int Id { get; set; }
        public int? ContractId { get; set; }
        public string? ReportNumber { get; set; }
        public DateTime? Date { get; set; }
        public string? CompanyName { get; set; }
        public string? ClientName { get; set; }
        public string? TechName { get; set; }
        public string? PhoneNum { get; set; }
        public string? Address { get; set; }
        public int? SerialNumberOfLift { get; set; }
        public int? VisitNumber { get; set; }
        public int? Capacity { get; set; }
        public string? LiftType { get; set; }
        public string? LiftModel { get; set; }
        public string? VisitType { get; set; }
        public string? ReasonOfVisit { get; set; }
        public string? Notes { get; set; }
        public string? ForTheNotWorkingItems { get; set; }
        public string? ClientSignaturePath { get; set; }
        public string? TechSignaturePath { get; set; }
        public int CheckingItemsCount { get; set; }
        public List<PeriodicMaintanceSparePartDto> SpareParts { get; set; } = new();
        public List<string>? Images { get; set; }
    }

    public class PeriodicMaintanceReportDetailDto : PeriodicMaintanceReportDto
    {
        public string? ContractDetails { get; set; }
        public List<PeriodicMaintanceCheckingItemDto> checkingItems { get; set; } = new();
    }

    public class PeriodicMaintanceCheckingItemDto
    {
        public int CheckingItemId { get; set; }
        public string Item { get; set; }
        public string Type { get; set; }
        public bool? Working { get; set; }
    }
}
