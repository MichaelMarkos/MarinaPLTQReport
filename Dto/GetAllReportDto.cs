
namespace maria.Dto
{
    public class GetAllReportDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string ReportNumber { get; set; }
        public string ReportType { get; set; }
        public string? InvoiceNumber { get; set; }
        public string CompanyName { get; set; }
        public string? ProjectAddress { get; set; }
        public string EquipmentType { get; set; }
        public string ModelMarnia { get; set; }
        public string ModelMarniaHireOrSale { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public string? WarrantyStatus { get; set; }
        public string specifications { get; set; }
        public string? ReasonOfVisitJson { get; set; }
        public string? spareParts { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string ClientSignaturePath { get; set; }
        public string TechSignaturePath { get; set; }
        public string ClientName { get; set; }
        public string TechName { get; set; }

        public List<string>? Images { get; set; } 

    }
}
