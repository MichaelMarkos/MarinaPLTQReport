using maria.Model;

namespace maria.Dto
{
    public class SiteReportDetailDto
    {
        public int Id { get; set; }
        public string? CompanyName { get; set; }
        public DateTime? Date { get; set; }
        public string? ClientSignaturePath { get; set; }
        public string? TechSignaturePath { get; set; }
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
