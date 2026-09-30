using System.ComponentModel.DataAnnotations.Schema;

namespace maria.Model
{
    public class PeriodicMaintanceItems
    {
        public int Id { get; set; }
        [ForeignKey("PeriodicMaintanceCheckingItems")]
        public int CheckingItemId { get; set; }
        [ForeignKey("PeriodicMaintanceReport")]
        public int PeriodicMaintanceReportId { get; set; }
        public bool Working { get; set; }

      
        public PeriodicMaintanceReport PeriodicMaintanceReport { get; set; }
        public PeriodicMaintanceCheckingItems PeriodicMaintanceCheckingItems { get; set; }
    }
}
