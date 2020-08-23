using System.Collections.Generic;
using System.Net;

namespace TwitCasting.Net.Downloader.WebSocket
{
    public interface IWebSocketOptions
    {
        public IDictionary<string, string> Headers { get; set; }

        IWebProxy WebProxy { get; set; }
    }
}