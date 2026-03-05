using System.ComponentModel.DataAnnotations.Schema;

namespace maria.Model
{
    public class EquipmentOfLevator
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        [ForeignKey("facade")]
        public int FacadeId { get; set; }
        public Facade facade { get; set; }

    }
}
