using System.Threading.Tasks;
using TwitCasting.Net.Downloader.EventArgs;

namespace TwitCasting.Net.Downloader.Common
{
    public delegate Task AsyncEventHandler();

    public delegate Task AsyncEventHandler<in T>(T e) where T : AsyncEventArgs;
}