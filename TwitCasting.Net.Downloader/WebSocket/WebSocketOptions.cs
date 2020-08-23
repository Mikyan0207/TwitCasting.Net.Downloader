using System.Collections.Generic;
using System.Net;

namespace TwitCasting.Net.Downloader.WebSocket
{
    public class WebSocketOptions : IWebSocketOptions
    {
        public IDictionary<string, string> Headers { get; set; }
        public IWebProxy WebProxy { get; set; }
    }
}