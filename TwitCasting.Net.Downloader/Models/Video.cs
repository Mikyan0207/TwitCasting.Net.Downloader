using Newtonsoft.Json;

namespace TwitCasting.Net.Downloader.Models
{
    public class Video
    {
        [JsonProperty("startTime")]
        public int StartTime { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("source")]
        public Source Source { get; set; }

        [JsonProperty("thumbnailUrl")]
        public string ThumbnailUrl { get; set; }
    }
}