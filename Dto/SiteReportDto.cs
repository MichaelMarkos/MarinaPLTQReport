namespace maria.Dto
{
    public class SiteReportDto
    {
        public int Id { get; set; }
        public string? ReportNumber { get; set; }
        public string? InvoiceNumber { get; set; }
        public string? CompanyName { get; set; }
        public DateTime? Date { get; set; }
        public string? PhoneNum { get; set; }
        public string? ReportType { get; set; }
        public string? Projectlocation { get; set; }
        public string? ProjectDescription { get; set; }
        public string? ClientSignaturePath { get; set; }
        public string? TechSignaturePath { get; set; }
        public string? ClientName { get; set; }
        public string? TechName { get; set; }
        public int CheckingItemsCount { get; set; }
        public List<string>? Images { get; set; }


    }
}
