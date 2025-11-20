using System.ComponentModel.DataAnnotations.Schema;

namespace maria.Model
{
    public class SafetyItemsReport
    {
        public int Id { get; set; }
        [ForeignKey("safetyItems")]
        public int SafetyItemsId { get; set; }
        [ForeignKey("safetyReport")]
        public int SafetyReportId { get; set; }
        public string CorrectiveAction { get; set; }

        public bool faultFlag { get; set; }
        public SafetyReport safetyReport { get; set; }
        public SafetyItems safetyItems { get; set; }
    }
}
