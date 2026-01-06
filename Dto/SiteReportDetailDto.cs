using maria.Model;

namespace maria.Dto
{
    public class SiteReportDetailDto
    {
        public int Id { get; set; }
        public string? CompanyName { get; set; }
        public string? ReportType { get; set; }
        public string? Projectlocation { get; set; }
        public string? ProjectDescription { get; set; }
        public string? PhoneNum { get; set; }
        public DateTime? Date { get; set; }
        public string? ClientName { get; set; }
        public string? TechName { get; set; }
        public string? ClientSignaturePath { get; set; }
        public string? TechSignaturePath { get; set; }
        public List<string>? Images { get; set; }
        public List<CheckingItemsDto> checkingItems { get; set; }
    }


    public class CheckingItemsDto
    {
        public string Item { get; set; }
        public string? CorrectiveAction { get; set; }
        public string? fault { get; set; }
        public bool CorrectiveActionFlag { get; set; }
        public bool faultFlag { get; set; }
        public bool Review { get; set; }
    }
}
