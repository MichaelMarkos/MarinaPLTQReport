namespace maria.Dto
{
    public class DeliveryReportDetailForUpdateDto
    {
        public string ReportType { get; set; }
        public DateTime? Date { get; set; }
        public string CompanyName { get; set; }
        public string PhoneNum { get; set; }
        public string ClientName { get; set; }
        public int? ContractId { get; set; }

        public string TechName { get; set; }

        public string Notes { get; set; }
        public string? ProjectAddress { get; set; }

        public List<DeliveryItemForUpdateDto> Items { get; set; }
        public List<DeliveryItemForUpdateDto> Items1 { get; set; }
        public List<DeliveryItemForUpdateDto> Items2 { get; set; }
        public List<DeliveryItemForUpdateDto> Items3 { get; set; }
        public List<DeliveryItemForUpdateDto> Items4 { get; set; }
        public List<DeliveryItemForUpdateDto> Items5 { get; set; }
        public List<DeliveryItemForUpdateDto> Scissorlifts { get; set; }
        public List<DeliveryItemForUpdateDto> manliftList { get; set; }
        public List<DeliveryItemForUpdateDto> productList { get; set; }

        public List<string> Images { get; set; }
    }
}
