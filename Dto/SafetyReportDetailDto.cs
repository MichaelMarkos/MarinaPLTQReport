namespace maria.Dto
{
    public class SafetyReportDetailDto
    {
        public int Id { get; set; }
        public string? CompanyName { get; set; }
        public int? ContractId { get; set; }
        public string? ContractDetails { get; set; }
        public string TechName { get; set; }
        public string? PhoneNum { get; set; }
        public string? ReportNumber { get; set; }
        public string? InvoiceNumber { get; set; }
        public int? TeamNum { get; set; }
        public int? TeamLeaderNum { get; set; }
        public string? TeamLeaderName { get; set; }

        public DateTime? Date { get; set; }
        public string? ClientSignaturePath { get; set; }
        public string? TechSignaturePath { get; set; }
        public List<CheckingSafetyItemsDto> checkingItems { get; set; }
    }


    public class CheckingSafetyItemsDto
    {
        public string Item { get; set; }
        public string? CorrectiveAction { get; set; }
        public string? fault { get; set; }
        public bool CorrectiveActionFlag { get; set; }
        public bool faultFlag { get; set; }
        public bool Review { get; set; }
    }
}
    

