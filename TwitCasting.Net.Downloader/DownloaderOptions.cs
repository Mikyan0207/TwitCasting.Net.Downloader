using CommandLine;

namespace TwitCasting.Net.Downloader
{
    public class DownloaderOptions
    {
        [Option('o', "output", Default = "out.mp4")]
        public string FileName { get; set; }

        public string TemporaryFile { get; set; }

        [Option('u', "user", Required = true)]
        public string TwitCaster { get; set; }

        [Option('q', "quality", Default = "hd1080")]
        public string Quality { get; set; }

        [Option('e', "encoding", Default = "hevc")]
        public string Encoding { get; set; }
    }
}