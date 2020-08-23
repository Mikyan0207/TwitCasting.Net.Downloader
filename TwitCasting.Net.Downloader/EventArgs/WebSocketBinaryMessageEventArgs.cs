namespace TwitCasting.Net.Downloader.EventArgs
{
    public sealed class WebSocketBinaryMessageEventArgs : WebSocketMessageEventArgs
    {
        public byte[] Message { get; }

        public WebSocketBinaryMessageEventArgs(byte[] bytes)
        {
            Message = bytes;
        }
    }
}