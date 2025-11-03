namespace maria.Model
{
    public class CheckingItem
    {
        public int Id { get; set; }
        public string Item { get; set; }
        public ICollection<CheckingItemReport> checkingItemReport { get; set; } = new List<CheckingItemReport>();

    }
}
