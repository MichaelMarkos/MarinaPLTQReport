using System.ComponentModel.DataAnnotations.Schema;

namespace maria.Model
{
    public class SiteReportImage
    {
        public int Id { get; set; }
        [ForeignKey("siteReports")]
        public int siteReportId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
      //  [JsonIgnore]
        public SiteReport siteReports { get; set; }
    }
}
