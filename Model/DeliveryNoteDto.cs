namespace maria.Model
{
    public class DeliveryNoteDto
    {
        public int checkingItemId { get; set; }
        public string quantity { get; set; }
        public string? unit { get; set; }
    }


    public class scissorliftsDto
    {
        public string model { get; set; }
        public string heightModel { get; set; }
        public string? quantity { get; set; }
    }

    public class productListDto
    {
        public string description { get; set; }
        public string quantity { get; set; }
    }
}
