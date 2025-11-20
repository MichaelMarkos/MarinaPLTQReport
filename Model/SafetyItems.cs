namespace maria.Model
{
    public class SafetyItems
    {
        public int Id { get; set; }
        public string Item { get; set; }
        public ICollection<SafetyItemsReport> safetyItemsReport { get; set; } = new List<SafetyItemsReport>();
    }
}
