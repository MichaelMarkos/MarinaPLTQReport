using System.ComponentModel.DataAnnotations.Schema;

namespace maria.Model
{
    public class LevatorReport
    {
        public int Id { get; set; }
        public int? ContractId { get; set; }
        //public string? ReportType { get; set; }
        public string? Projectlocation { get; set; }
        public string? ProjectDescription { get; set; }
        public long UserId { get; set; } = 1;
        public string? ReportNumber { get; set; }
        public string? InvoiceNumber { get; set; }
        public string? BuildingType { get; set; }
        public string? BuildingKind { get; set; }
        //public string? ClientSignaturePath { get; set; }
        //public string? TechSignaturePath { get; set; }
        public DateTime Date { get; set; }
        public string? CompanyName { get; set; }
        public string? salesperson { get; set; }
        public string? TechName { get; set; }
        public string? PhoneNum { get; set; }
        [Column(TypeName = "decimal(18,8)")]
        public decimal? x { get; set; }

        [Column(TypeName = "decimal(18,8)")]
        public decimal? y { get; set; }

        public ICollection<Facade> facades { get; set; } = new List<Facade>();
        public ICollection<LevatorImage> LevatorImages { get; set; } = new List<LevatorImage>();
    }
}
