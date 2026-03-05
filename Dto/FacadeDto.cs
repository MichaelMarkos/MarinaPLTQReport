using maria.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace maria.Dto
{
    public class FacadeDto
    {
        public int id { get; set; }
        public int number { get; set; }
        public string? typeOfFinish { get; set; }
        public string? typeOfWall { get; set; }
        public string? typeOfLand { get; set; }
        public decimal? height { get; set; }
        public decimal? heightWall { get; set; }
        public decimal? width { get; set; }
        public decimal? max { get; set; }
        public bool isSpecial { get; set; }
        public string? notes { get; set; }
        [ForeignKey("LevatorReport")]
        public int LevatorReportId { get; set; }
        public LevatorReport LevatorReport { get; set; }
        public ICollection<ScaffoldDto> scaffolds { get; set; } = new List<ScaffoldDto>();
        public List<EquipmentDto> equipments { get; set; } = new List<EquipmentDto>();
        public List<ImageDto> images { get; set; }

    }


    public class EquipmentDto
    {
        public int id { get; set; }
        public string type { get; set; }
        public string description { get; set; }
    }

    public class ScaffoldDto
    {
        public int id { get; set; }
        public string? typeOfUse { get; set; }
        public string? typeOfGroup { get; set; }
        public string? setGroup { get; set; }
        public string? model { get; set; }
        public string? specialText { get; set; }
        public string? typeBox { get; set; }
        public string? heightBox { get; set; }
        public decimal? widthBox { get; set; }
        public int? numberTransfers { get; set; }
        public int? wirelength { get; set; }
        public int? electricWirelength { get; set; }
        public string? powerSource { get; set; }
        public string? liftingLoads { get; set; }
        public string? notes { get; set; }
    }

    public class ImageDto
    {
        public int id { get; set; }
        public string? fileName { get; set; }
        public string? filePath { get; set; }
    }

    public class ReportDetailsDto
    {
        public int id { get; set; }
        public string? projectlocation { get; set; }
        public string? projectDescription { get; set; }
        public string? reportNumber { get; set; }
        public string? invoiceNumber { get; set; }
        public string? buildingType { get; set; }
        public string? buildingKind { get; set; }
        public string? companyName { get; set; }
        public DateTime? date { get; set; }
        public string? salesperson { get; set; }
        public string? techName { get; set; }
        public string? phoneNum { get; set; }
        public decimal? x { get; set; }
        public decimal? y { get; set; }
        public List<FacadeDto> facades { get; set; } = new List<FacadeDto>();
        public List<ImageDto> images { get; set; } = new List<ImageDto>();
    }
}
