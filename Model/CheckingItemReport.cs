using System.ComponentModel.DataAnnotations.Schema;

namespace maria.Model
{
    public class CheckingItemReport
    {
        public int Id { get; set; }
        [ForeignKey("checkingItem")]
        public int CheckingItemId { get; set; }
        [ForeignKey("siteReport")]
        public int SiteReportId { get; set; }
        public string CorrectiveAction   { get; set; }
        public string fault { get; set; }

        public bool CorrectiveActionFlag { get; set; }
        public bool faultFlag { get; set; }
        public SiteReport siteReport { get; set; }
        public CheckingItem checkingItem { get; set; }

    }
}
