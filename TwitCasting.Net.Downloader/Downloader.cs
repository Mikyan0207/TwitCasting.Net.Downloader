using AngleSharp.Html.Parser;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitCasting.Net.Downloader.Common;
using TwitCasting.Net.Downloader.Internal;
using TwitCasting.Net.Downloader.Models;
using Uri = System.Uri;

namespace TwitCasting.Net.Downloader
{
    public class Downloader : TwitCastingClient
    {
        private static HtmlParser Parser { get; set; } = new HtmlParser();

        public Downloader(Options options) : base(options)
        {
        }

        public async Task DownloadAsync()
        {
            var body = await Client
                .GetStringAsync(new Uri(Endpoints.Live.Replace("{user}", Options.TwitCaster)
                    .Replace("{liveId}", Options.LiveId))).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(body))
                throw new TaskCanceledException("Invalid URL.");

            var html = Parser.ParseDocument(body);
            var movieData = html.QuerySelector("video").GetAttribute("data-movie-playlist");

            if (string.IsNullOrWhiteSpace(movieData))
                throw new TaskCanceledException("Invalid URL.");

            var json = JsonConvert.DeserializeObject<List<Video>>(movieData).FirstOrDefault();

            if (json == null)
                throw new TaskCanceledException("Invalid URL.");

            await Converter.FFMpegDownload(json.Source.Url, Options.FileName).ConfigureAwait(false);
        }
    }
}