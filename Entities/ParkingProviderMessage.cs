using System.Text.Json.Serialization;

namespace MachogPatch.Entities
{
    public class ParkingProviderMessage
    {
        [JsonPropertyName("vehicle_number")]
        public required string VehicleNumber { get; set; }
        [JsonPropertyName("provider")]
        public int ParkingProvider { get; set; }
        [JsonPropertyName("app_id")]
        public required string AppID { get; set; }
    }
}
