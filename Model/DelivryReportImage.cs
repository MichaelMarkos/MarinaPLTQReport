using System.ComponentModel.DataAnnotations.Schema;

namespace maria.Model
{
    public class DelivryReportImage
    {
        public int Id { get; set; }
        [ForeignKey("deliveryReports")]
        public int deliveryReportId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        //  [JsonIgnore]
        public DeliveryReport deliveryReports { get; set; }
    }
}
