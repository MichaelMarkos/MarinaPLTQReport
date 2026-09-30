using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace maria.Model
{
    // قطع الغيار (Spare parts) الخاصة بتقرير الزيارة الدورية
    public class PeriodicMaintanceSparePart
    {
        public int Id { get; set; }
        [ForeignKey("PeriodicMaintanceReport")]
        public int PeriodicMaintanceReportId { get; set; }
        public string Name { get; set; }
        public int? Quantity { get; set; }
        // false = مطلوب (Required) ، true = تم التوريد (Supplied)
        public bool IsSupplied { get; set; }

        [JsonIgnore]
        public PeriodicMaintanceReport PeriodicMaintanceReport { get; set; }
    }
}
