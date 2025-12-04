namespace maria.Dto
{
    public class SiteReportUpdateDto
    {
        public int Id { get; set; }
        public string? CompanyName { get; set; }
        public string TechName { get; set; }
        public string? PhoneNum { get; set; }
        public string? ReportNumber { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateTime? Date { get; set; }
    }
}
