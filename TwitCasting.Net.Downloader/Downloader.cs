using System.Threading.Tasks;

namespace TwitCasting.Net.Downloader
{
    public class Downloader : TwitCastingClient
    {
        public Downloader(Options options) : base(options)
        {
        }

        public async Task DownloadAsync()
        {
        }
    }
}