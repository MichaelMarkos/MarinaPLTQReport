namespace maria.Model
{
    public class SiteReport
    {
        public int Id { get; set; }
        public long UserId { get; set; } = 1;
        public string? ReportNumber { get; set; }
        public string? ClientSignaturePath { get; set; }
        public string? TechSignaturePath { get; set; }
        public DateTime Date { get; set; }
        public string CompanyName { get; set; }
        public string ClientName { get; set; }
        public string TechName { get; set; }
        public string? PhoneNum { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<CheckingItemReport> checkingItemReport { get; set; } = new List<CheckingItemReport>();

    }
}
