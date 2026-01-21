namespace maria.Dto
{
    public class DeliveryReportDetailDto
    {

        public int Id { get; set; }
        public string? ReportType { get; set; }
        public string? ReportNumber { get; set; }
        public string? CompanyName { get; set; }
        public string? ProjectAddress { get; set; }
        public DateTime? Date { get; set; }
        public string? ClientSignaturePath { get; set; }
        public string? TechSignaturePath { get; set; }
        public string? Notes { get; set; }
        public List<string>? Images { get; set; }

        public List<DeliveryItemsDto> checkingItems { get; set; }


    }
    public class DeliveryItemsDto
    {
        public int deliveryNoteId { get; set; }
        public string Description { get; set; }
        public string DeliveryType { get; set; }
        public int Quantity { get; set; }
        public string? Unit { get; set; }
    }

}
