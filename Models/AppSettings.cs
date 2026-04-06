using System.Text.Json.Serialization;

namespace LegaCity.Models
{
    public class AppSettings
    {
        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("fullscreen")]
        public bool Fullscreen { get; set; }
    }
}
