namespace FlashFit.Models
{
    public class CheckoutRequest
    {
        public List<string> BillingTypes { get; set; }
        public List<string> ChargeTypes { get; set; }
        public Callback Callback { get; set; }
        public List<Item> Items { get; set; }
    }

    public class Callback
    {
        public string SuccessUrl { get; set; }
        public string CancelUrl { get; set; }
        public string ExpiredUrl { get; set; }
    }

    public class Item
    {
        public string Description { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Value { get; set; }
    }

}
