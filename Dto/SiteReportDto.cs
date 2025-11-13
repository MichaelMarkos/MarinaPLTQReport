namespace maria.Dto
{
    public class SiteReportDto
    {
        public int Id { get; set; }
        public string? CompanyName { get; set; }
        public DateTime? Date { get; set; }
        public string? ClientSignaturePath { get; set; }
        public string? TechSignaturePath { get; set; }
        public string ClientName { get; set; }
        public string TechName { get; set; }
        public int CheckingItemsCount { get; set; }
        public string? ReportNumber { get; set; }

    }
}
