namespace maria.Model
{
    public class ElevatorChechingItems
    {
        public int Id { get; set; }
        public string Item { get; set; }
        public string Type { get; set; }
        public ICollection<ElevatorInspectionItems> elevatorInspectionItems { get; set; } = new List<ElevatorInspectionItems>();

    }
}
