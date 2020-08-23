using System;

namespace TwitCasting.Net.Downloader.EventArgs
{
    public sealed class WebSocketErrorEventArgs : AsyncEventArgs
    {
        public Exception Exception { get; }

        public WebSocketErrorEventArgs(Exception ex)
        {
            Exception = ex;
        }
    }
}