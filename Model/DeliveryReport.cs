namespace maria.Model
{
    public class DeliveryReport
    {
        public int Id { get; set; }
        public long UserId { get; set; } = 1;
        public string? ClientSignaturePath { get; set; }
        public string? TechSignaturePath { get; set; }
        public DateTime Date { get; set; }
        public string? ReportNumber { get; set; }
        public string? InvoiceNumber { get; set; }
        public string CompanyName { get; set; }
        public string Notes { get; set; }

        public string? ProjectAddress { get; set; }
        public string ClientName { get; set; }
        public string TechName { get; set; }

        public ICollection<DeliveryNoteReport> checkingItemReport { get; set; } = new List<DeliveryNoteReport>();
    }
}
