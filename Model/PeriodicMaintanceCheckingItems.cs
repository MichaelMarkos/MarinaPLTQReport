namespace maria.Model
{
    public class PeriodicMaintanceCheckingItems
    {
        public int Id { get; set; }
        public string Item { get; set; }
        public string Type { get; set; }
        public ICollection<PeriodicMaintanceItems> PeriodicMaintanceItems { get; set; } = new List<PeriodicMaintanceItems>();
    }
}
