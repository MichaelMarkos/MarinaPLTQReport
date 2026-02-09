using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace maria.Model
{
    public class ElevatorInspectionImage
    {
        public int Id { get; set; }
        [ForeignKey("ElevatorInspectionReports")]
        public int ElevatorInspectionId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        [JsonIgnore]
        public ElevatorInspectionReport ElevatorInspectionReports { get; set; }
    }
}
