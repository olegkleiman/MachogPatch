using System.Text.Json.Serialization;

namespace MachogPatch.Services.ParkingProviderService
{
    public class ParkingProviderOptions
    {
        [JsonPropertyName("url")]
        public required string Url { get; set; }

        [JsonPropertyName("subscriptionHeader")]
        public required string SubscriptionHeader { get; set; }

        [JsonPropertyName("headerValue")]
        public required string HeaderValue { get; set; }
    }
}
