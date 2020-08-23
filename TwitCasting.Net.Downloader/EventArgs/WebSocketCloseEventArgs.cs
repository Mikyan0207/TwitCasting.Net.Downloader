namespace TwitCasting.Net.Downloader.EventArgs
{
    public sealed class WebSocketCloseEventArgs : AsyncEventArgs
    {
        public int ErrorCode { get; }

        public string Message { get; }

        public WebSocketCloseEventArgs(int errorCode, string message)
        {
            ErrorCode = errorCode;
            Message = message;
        }
    }
}