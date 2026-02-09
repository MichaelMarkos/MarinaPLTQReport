using System.ComponentModel.DataAnnotations.Schema;

namespace maria.Model
{
    public class ElevatorInspectionItems
    {
        public int Id { get; set; }
        [ForeignKey("ElevatorChechingItems")]
        public int CheckingItemId { get; set; }
        [ForeignKey("elevatorInspectionReport")]
        public int ElevatorInspectionReportId { get; set; }
        public string CorrectiveAction { get; set; }
        public string fault { get; set; }

        public bool CorrectiveActionFlag { get; set; }
        public bool faultFlag { get; set; }
        public ElevatorInspectionReport elevatorInspectionReport { get; set; }
        public ElevatorChechingItems ElevatorChechingItems { get; set; }
    }
}
