namespace maria.Dto
{
    public class SafetyReportDetailDto
    {
        public int Id { get; set; }
        public string? CompanyName { get; set; }
        public DateTime? Date { get; set; }
        public string? ClientSignaturePath { get; set; }
        public string? TechSignaturePath { get; set; }
        public List<CheckingSafetyItemsDto> checkingItems { get; set; }
    }


    public class CheckingSafetyItemsDto
    {
        public string Item { get; set; }
        public string? CorrectiveAction { get; set; }
        public bool faultFlag { get; set; }
        public bool Review { get; set; }
    }
}
    

