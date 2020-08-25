using Newtonsoft.Json;

namespace TwitCasting.Net.Downloader.Models
{
    public class Source
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}