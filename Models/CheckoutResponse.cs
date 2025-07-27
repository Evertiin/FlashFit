using System.Text.Json.Serialization;

namespace FlashFit.Models
{
    public class CheckoutResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("link")]
        public string Link { get; set; }
    }
}
