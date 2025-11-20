using System.Text.Json.Serialization;

namespace maria.Model
{
    public class ElevatorImage
    {
        public int Id { get; set; }
        public int ElevatorId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        [JsonIgnore]
        public Elevator Elevator { get; set; }
    }
}
