namespace maria.Model
{
    public class DeliveryNote
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string DeliveryType { get; set; }
        public bool UnitFlag { get; set; }
        public bool OptionalFlag { get; set; }
        public ICollection<DeliveryNoteReport> deliveryNoteReport { get; set; } = new List<DeliveryNoteReport>();

    }
}
