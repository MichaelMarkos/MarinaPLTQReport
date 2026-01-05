namespace maria.Dto
{
    public class DeliveryItemDto
    {
        public int Id { get; set; }
        public int DeliveryNoteId { get; set; }
        public string Item { get; set; } = "";
        public int Quantity { get; set; }
        public bool UnitFlag { get; set; }
        public string Unit { get; set; } = "";
        public bool IsModified { get; set; }
    }
    public class DeliveryItemForUpdateDto
    {
        public int Id { get; set; }
        public int checkingItemId { get; set; }
        public string Item { get; set; } = "";
        public int Quantity { get; set; }
        public bool UnitFlag { get; set; }
        public string Unit { get; set; } = "";

    }
   




}
