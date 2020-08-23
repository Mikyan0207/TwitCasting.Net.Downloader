using CommandLine;
using System;
using System.IO;
using System.Threading.Tasks;
using TwitCasting.Net.Downloader.Internal;

namespace TwitCasting.Net.Downloader
{
    public class Program
    {
        private static Downloader Downloader { get; set; }

        private static DownloaderOptions Options { get; set; }

        private static volatile bool _exitRequested = false;

        public static async Task Main(string[] args)
        {
            Options = Parse<DownloaderOptions>(args);
            Options.FileName = Path.GetFileNameWithoutExtension(Options.FileName); // Remove extension if any
            Options.TemporaryFile = $"{Path.GetFileNameWithoutExtension(Options.FileName)}-tmp.ts";
            Downloader = new Downloader(Options);

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                _exitRequested = true;
            };

            try
            {
                await Downloader.DownloadStreamAsync(Options.TwitCaster).ConfigureAwait(false);

                while (Downloader.IsDownloading && !_exitRequested) { } // ...?

                await Downloader.DisconnectAsync().ConfigureAwait(false);
                await Converter.Convert(Options);
            }
            catch (TaskCanceledException)
            {
                Logger.DownloaderLog($"{Options.TwitCaster} is not streaming");
            }
            catch (Exception e)
            {
                Logger.DownloaderLog("Unknown error, please retry " + e.Message);
            }
        }

        public static T Parse<T>(string[] args) where T : class, new()
        {
            using var parser = new Parser();
            var result = parser.ParseArguments<T>(args);
            var options = new T();

            result.WithParsed(x => options = x);

            return options;
        }
    }
}