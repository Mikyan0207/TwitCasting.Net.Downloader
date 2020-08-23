namespace TwitCasting.Net.Downloader.EventArgs
{
    public abstract class AsyncEventArgs : System.EventArgs
    {
        public bool Handled { get; set; }
    }
}