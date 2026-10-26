using System.Text.Json.Serialization;

namespace maria.Model
{
    public class ElevatorSpecificationImage
    {
        public int Id { get; set; }
        public int ElevatorSpecificationId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        [JsonIgnore]
        public ElevatorSpecification ElevatorSpecification { get; set; }
    }
}
