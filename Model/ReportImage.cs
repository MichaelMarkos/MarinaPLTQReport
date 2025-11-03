using System.Text.Json.Serialization;

namespace maria.Model
{
    public class ReportImage
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        [JsonIgnore]
        public Report Report { get; set; }
    }
}
