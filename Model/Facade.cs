using System.ComponentModel.DataAnnotations.Schema;

namespace maria.Model
{
    public class Facade
    {
        public int Id { get; set; }
        public int Number { get; set; }            
        public string? TypeOfFinish { get; set; }          
        public string? TypeOfWall { get; set; }          
        public string? TypeOfLand { get; set; }          
        public decimal? Height { get; set; }        
        public decimal? heightWall { get; set; }        
        public decimal? Width { get; set; }       
        public decimal? Max { get; set; }       
        public bool IsSpecial { get; set; }       
        public string? Notes { get; set; }
        [ForeignKey("LevatorReport")]
        public int LevatorReportId { get; set; }
        public LevatorReport LevatorReport { get; set; }
        public ICollection<Scaffold> Scaffolds { get; set; } = new List<Scaffold>();
    }
}
