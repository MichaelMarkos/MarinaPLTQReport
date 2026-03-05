
using System.ComponentModel.DataAnnotations.Schema;

namespace maria.Model
{
    public class Scaffold
    {
        public int Id { get; set; }

        [ForeignKey("facade")]
        public int FacadeId { get; set; }

        public string? TypeOfUse { get; set; }
        public string? TypeOfGroup { get; set; }
        public string? SetGroup { get; set; }
        public string? SpecialText { get; set; }
        public string? Model { get; set; }
        public string? TypeBox { get; set; }
        public string? HeightBox { get; set; }
        public decimal? WidthBox { get; set; }
        public int? NumberTransfers  { get; set; }
        public int? Wirelength  { get; set; }
        public int? ElectricWirelength  { get; set; }
        public string? PowerSource { get; set; }
        public string? Liftingoods { get; set; }
        public string? Notes { get; set; }

         public Facade facade { get; set; }

    }
}
