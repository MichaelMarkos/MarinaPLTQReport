using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace maria.Model
{
    public class PeriodicMaintanceImage
    {
        public int Id { get; set; }
        [ForeignKey("PeriodicMaintanceReport")]
        public int PeriodicMaintanceReportId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        [JsonIgnore]
        public PeriodicMaintanceReport PeriodicMaintanceReport { get; set; }
    }
}
