namespace FlashFit.Models
{
    public class WebhookResponseDto
    {
        public string Id { get; set; }
        public string Event { get; set; }
        public string DateCreated { get; set; }
        public CheckoutDto Checkout { get; set; }
    }

    public class CheckoutDto
    {
        public string Id { get; set; }
        // Adicione outros campos conforme necessário
    }
}