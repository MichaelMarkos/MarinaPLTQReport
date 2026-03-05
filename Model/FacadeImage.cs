using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace maria.Model
{
    public class FacadeImage
    {
        public int Id { get; set; }
        [ForeignKey("facade")]
        public int FacadeId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        [JsonIgnore]
        public Facade facade { get; set; }
    }
}
