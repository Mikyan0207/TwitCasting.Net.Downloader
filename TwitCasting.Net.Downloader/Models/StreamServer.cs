using Newtonsoft.Json;

namespace TwitCasting.Net.Downloader.Models
{
    public class Movie
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("live")]
        public bool Live { get; set; }
    }

    public class Hls
    {
        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("proto")]
        public string Proto { get; set; }

        [JsonProperty("source")]
        public bool Source { get; set; }
    }

    public class Fmp4
    {
        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("proto")]
        public string Proto { get; set; }

        [JsonProperty("source")]
        public bool Source { get; set; }

        [JsonProperty("mobilesource")]
        public bool MobileSource { get; set; }
    }

    public class StreamServer
    {
        [JsonProperty("movie")]
        public Movie Movie { get; set; }

        [JsonProperty("hls")]
        public Hls Hls { get; set; }

        [JsonProperty("fmp4")]
        public Fmp4 Fmp4 { get; set; }
    }
}