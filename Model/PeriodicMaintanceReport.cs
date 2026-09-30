namespace maria.Model
{
    public class PeriodicMaintanceReport
    {
        public int Id { get; set; }
        public int? ContractId { get; set; }
        public int? SerialNumberOfLift { get; set; }
        public int?  VisitNumber { get; set; }
        public int? Capacity { get; set; }

        //public string? ReportType { get; set; }
        public string? LiftType { get; set; }
        public string? LiftModel { get; set; }
        public string? VisitType { get; set; }
        public string? ReasonOfVisit { get; set; }
        public long UserId { get; set; } = 1;
        public string? ReportNumber { get; set; }
        public string? Address { get; set; }
        public string? ClientSignaturePath { get; set; }
        public string? TechSignaturePath { get; set; }
        public DateTime Date { get; set; }
        public string? CompanyName { get; set; }
        public string? ClientName { get; set; }
        public string? TechName { get; set; }
        public string? PhoneNum { get; set; }
        public string? Notes { get; set; }
        public string? ForTheNotWorkingItems { get; set; }
        public ICollection<PeriodicMaintanceItems> PeriodicMaintanceItems { get; set; } = new List<PeriodicMaintanceItems>();
        public ICollection<PeriodicMaintanceSparePart> SpareParts { get; set; } = new List<PeriodicMaintanceSparePart>();
        public ICollection<PeriodicMaintanceImage> Images { get; set; } = new List<PeriodicMaintanceImage>();
    }
}
