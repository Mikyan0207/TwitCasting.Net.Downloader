using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TwitCasting.Net.Downloader.Common;
using TwitCasting.Net.Downloader.EventArgs;
using TwitCasting.Net.Downloader.Internal;
using TwitCasting.Net.Downloader.Models;
using TwitCasting.Net.Downloader.WebSocket;

namespace TwitCasting.Net.Downloader
{
    public partial class Recorder : TwitCastingClient
    {
        private static WebSocketClient WebSocketClient { get; set; }

        private static FileStream FileStream { get; set; }

        private ulong MessageCount { get; set; }

        public bool IsRecording { get; private set; }

        public Recorder(Options options) : base(options)
        {
            FileStream = new FileStream($"{Options.TemporaryFile}.mp4", FileMode.Append, FileAccess.Write);
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

        public async Task RecordAsync()
        {
            var ss = await GetStreamServerAsync(Endpoints.StreamServer,
                new Dictionary<string, string> { { "target", Options.TwitCaster }, { "mode", "client" } }).ConfigureAwait(false);

            if (!ss.Movie.Live)
                throw new TaskCanceledException($"{Options.TwitCaster} is not streaming");

            // Add Headers
            WebSocketClient.AddDefaultHeader("Host", $"{ss.Fmp4.Host}");

            await ConnectAsync(ss).ConfigureAwait(false);
        }

        public async Task DisconnectAsync()
        {
            await WebSocketClient.DisconnectAsync().ConfigureAwait(false);
        }

        private async Task ConnectAsync(StreamServer ss)
        {
            await WebSocketClient
                .ConnectAsync(new System.Uri(
                    $"wss://{ss.Fmp4.Host}/ws.app/stream/{ss.Movie.Id}/fmp4/bd/1/1500?mode=source"))
                .ConfigureAwait(false);

            IsRecording = true;
        }

        private async Task<StreamServer> GetStreamServerAsync(string url, IDictionary<string, string> parameters)
            => await Get<StreamServer>(url, parameters).ConfigureAwait(false);
    }

    public partial class Recorder
    {
        private static Task WebSocketClient_Connected()
        {
            Logger.WebSocketLog("Connected!\n");

            return Task.CompletedTask;
        }

        private Task WebSocketClient_Disconnected(WebSocketCloseEventArgs e)
        {
            WebSocketClient.RemoveDefaultHeader("Host");
            Logger.WebSocketLog($"Disconnected!");
            IsRecording = false;

            return Task.CompletedTask;
        }

        private Task WebSocketClient_MessageReceived(WebSocketMessageEventArgs e)
        {
            switch (e)
            {
                case WebSocketTextMessageEventArgs tme:
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
            IsRecording = false;

            return Task.CompletedTask;
        }
    }
}