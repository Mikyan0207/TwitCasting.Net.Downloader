using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TwitCasting.Net.Downloader.EventArgs;
using TwitCasting.Net.Downloader.Internal;
using TwitCasting.Net.Downloader.Models;
using TwitCasting.Net.Downloader.WebSocket;
using Uri = TwitCasting.Net.Downloader.Internal.Uri;

namespace TwitCasting.Net.Downloader
{
    public partial class Downloader
    {
        private static WebSocketClient WebSocketClient { get; set; }

        private static HttpClient Client => LazyHttpClient.Value;

        private static DownloaderOptions Options { get; set; }

        private static FileStream FileStream { get; set; }

        private ulong MessageCount { get; set; }

        public bool IsDownloading { get; private set; }

        public Downloader(DownloaderOptions options)
        {
            Options = options;
            FileStream = new FileStream($"{Options.TemporaryFile}", FileMode.Append, FileAccess.Write);
            WebSocketClient = WebSocketClient.CreateNew(new WebSocketOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Origin", "https://twitcasting.tv" },
                    { "Connection", "Upgrade" },
                    { "Upgrade", "websocket" },
                    { "Sec-WebSocket-Extensions", "permessage-deflate; client_max_window_bits" },
                    { "Sec-WebSocket-Version", "13" },
                    { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.125 Safari/537.36" }
                },
                WebProxy = null
            });

            WebSocketClient.Connected += WebSocketClient_Connected;
            WebSocketClient.Disconnected += WebSocketClient_Disconnected;
            WebSocketClient.MessageReceived += WebSocketClient_MessageReceived;
            WebSocketClient.Errored += WebSocketClient_Errored;
        }

        public async Task DisconnectAsync()
        {
            await WebSocketClient.DisconnectAsync().ConfigureAwait(false);
        }

        private async Task<HttpResponseMessage> SendAsync(string url, IDictionary<string, string> parameters = null)
        {
            var response = await Client.SendAsync(CreateRequestMessage(url, parameters)).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            return response;
        }

        private async Task<HttpResponseMessage> GetAsync(string url, IDictionary<string, string> parameters = null)
            => await SendAsync(url, parameters).ConfigureAwait(false);

        private async Task<T> Get<T>(string url, IDictionary<string, string> parameters = null) where T : class
        {
            var response = await GetAsync(url, parameters).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return JsonConvert.DeserializeObject<T>(content);
        }

        private async Task ConnectAsync(StreamServer ss)
        {
            await WebSocketClient
                .ConnectAsync(new System.Uri(
                    $"wss://{ss.Fmp4.Host}/ws.app/stream/{ss.Movie.Id}/fmp4/bd/1/1500?mode=source"))
                .ConfigureAwait(false);

            IsDownloading = true;
        }
    }

    public partial class Downloader
    {
        private async Task<StreamServer> GetStreamServerAsync(string url, IDictionary<string, string> parameters)
            => await Get<StreamServer>(url, parameters).ConfigureAwait(false);

        public async Task DownloadStreamAsync(string userId)
        {
            var ss = await GetStreamServerAsync(Endpoints.StreamServer,
                new Dictionary<string, string> { { "target", userId }, { "mode", "client" } }).ConfigureAwait(false);

            if (!ss.Movie.Live)
                throw new TaskCanceledException($"{userId} is not streaming");

            // Add Headers
            WebSocketClient.AddDefaultHeader("Host", $"{ss.Fmp4.Host}");

            await ConnectAsync(ss).ConfigureAwait(false);
        }
    }

    public partial class Downloader
    {
        internal HttpRequestMessage CreateRequestMessage(string requestUri, IDictionary<string, string> parameters = null)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new System.Uri(Uri.BuildUri(requestUri, parameters)),
                Headers =
                {
                    Accept =
                    {
                        new MediaTypeWithQualityHeaderValue("text/html")
                    }
                }
            };

            return request;
        }
    }

    public partial class Downloader
    {
        private static readonly Lazy<HttpClient> LazyHttpClient = new Lazy<HttpClient>(() =>
        {
            var httpClient = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
            }, true);

            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.163 Safari/537.36");

            return httpClient;
        });
    }

    public partial class Downloader
    {
        private static Task WebSocketClient_Connected()
        {
            Logger.WebSocketLog("Connected!\n");

            return Task.CompletedTask;
        }

        private Task WebSocketClient_Disconnected(EventArgs.WebSocketCloseEventArgs e)
        {
            WebSocketClient.RemoveDefaultHeader("Host");
            Logger.WebSocketLog($"Disconnected!");
            IsDownloading = false;

            return Task.CompletedTask;
        }

        private Task WebSocketClient_MessageReceived(EventArgs.WebSocketMessageEventArgs e)
        {
            switch (e)
            {
                case WebSocketTextMessageEventArgs tme:
                    CheckWebSocketStatus(tme.Message);
                    break;

                case WebSocketBinaryMessageEventArgs bme:
                    Logger.UpdateLine("{0} " + $"{MessageCount++}" + " {3}");
                    FileStream.Write(bme.Message);
                    break;
            }

            return Task.CompletedTask;
        }

        private Task WebSocketClient_Errored(WebSocketErrorEventArgs e)
        {
            var ex = e.Exception;
            while (ex is AggregateException ae)
                ex = ae.InnerException;

            Logger.WebSocketLog($"Disconnected! (Canceled/Stream ended)");
            Logger.WebSocketLog(ex?.Message);
            IsDownloading = false;

            return Task.CompletedTask;
        }

        private static void CheckWebSocketStatus(string jsonStatus)
        {
        }
    }
}