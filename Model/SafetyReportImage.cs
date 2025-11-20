using System.ComponentModel.DataAnnotations.Schema;

namespace maria.Model
{
    public class SafetyReportImage
    {
        public int Id { get; set; }
        [ForeignKey("safetyReport")]
        public int safetyReportId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        //  [JsonIgnore]
        public SafetyReport safetyReport { get; set; }
    }
}
