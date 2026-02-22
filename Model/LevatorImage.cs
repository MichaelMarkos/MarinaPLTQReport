using System.Text.Json.Serialization;

namespace maria.Model
{
    public class LevatorImage
    {
        public int Id { get; set; }
        public int LevatorReportId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        [JsonIgnore]
        public LevatorReport LevatorReport { get; set; }
    }
}
