namespace maria.Dto
{
    public class SafetyReportDto
    {
        public int Id { get; set; }
        public string? CompanyName { get; set; }
        public DateTime? Date { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? ClientSignaturePath { get; set; }
        public string? TechSignaturePath { get; set; }
        public string ClientName { get; set; }
        public string TechName { get; set; }
        public int safetyItemsCount { get; set; }
        public string? ReportNumber { get; set; }
        public string? PhoneNum { get; set; }


        public string? InvoiceNumber { get; set; }

        public int TeamNum { get; set; }
        public int TeamLeaderNum { get; set; }
        public string TeamLeaderName { get; set; }
        public string ProjectDescription { get; set; }
        public string TeamMembers { get; set; }
        public string ProjectName { get; set; }
        public string SiteName { get; set; }
        public string Projectlocation { get; set; }
        public string? Notes { get; set; }


        public List<string>? Images { get; set; }
    }
}
