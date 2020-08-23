namespace TwitCasting.Net.Downloader.EventArgs
{
    public sealed class WebSocketTextMessageEventArgs : WebSocketMessageEventArgs
    {
        public string Message { get; }

        public WebSocketTextMessageEventArgs(string message)
        {
            Message = message;
        }
    }
}