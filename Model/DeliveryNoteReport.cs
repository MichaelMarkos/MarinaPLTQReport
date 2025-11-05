using System.ComponentModel.DataAnnotations.Schema;

namespace maria.Model
{
    public class DeliveryNoteReport
    {
        public int Id { get; set; }
        [ForeignKey("deliveryNote")]
        public int deliveryNoteId { get; set; }
        [ForeignKey("deliveryReport")]
        public int deliveryReportId { get; set; }
        public int Quantity { get; set; }
        public int? UnitValue { get; set; }

        public DeliveryReport deliveryReport { get; set; }
        public DeliveryNote deliveryNote { get; set; }
    }
}
