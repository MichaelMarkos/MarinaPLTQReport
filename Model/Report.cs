namespace maria.Model
{
    public class Report
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string ReportType { get; set; }
        public long UserId { get; set; } = 1;
        public string? ReportNumber { get; set; }
        public string? InvoiceNumber { get; set; }
        public string CompanyName { get; set; }
        public string? ProjectAddress { get; set; }
        public string EquipmentType { get; set; }
        public string ModelMarnia { get; set; }
        public string ModelMarniaHireOrSale { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public string? WarrantyStatus { get; set; }

        public int Cradle { get; set; }
        public int Meter { get; set; }
        public string Unit { get; set; }

        public int? Installation { get; set; }
        public int? Removing { get; set; }
        public int? Shifting { get; set; }
        public int? PeriodicMaintenance { get; set; }
        public int? ThirdParty { get; set; }
        public int? Breakdown { get; set; }
        public int? Inspection { get; set; }
        public int? Delivery { get; set; }
        public int? OnScaffolding { get; set; }

        public string? spareParts { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        //public string? PdfFilePath { get; set; } 

        public string ClientName { get; set; }
        public string ClientSignaturePath { get; set; }
        public string TechSignaturePath { get; set; }
        public string TechName { get; set; }
        public string? PhoneNum { get; set; }
        public ICollection<ReportImage> ReportFiles { get; set; } = new List<ReportImage>();

    }
}
