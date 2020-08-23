using CommandLine;
using System;
using System.IO;
using System.Threading.Tasks;
using TwitCasting.Net.Downloader.Internal;

namespace TwitCasting.Net.Downloader
{
    public class Program
    {
        private static Recorder Recorder { get; set; }

        private static Downloader Downloader { get; set; }

        private static Options Options { get; set; }

        private static volatile bool _exitRequested = false;

        public static async Task Main(string[] args)
        {
            Options = Parse<Options>(args);
            Options.FileName = Path.GetFileNameWithoutExtension(Options.FileName); // Remove extension if any

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                _exitRequested = true;
            };

            if (!Options.IsRecording)
            {
                if (string.IsNullOrWhiteSpace(Options.LiveId) || string.IsNullOrWhiteSpace(Options.TwitCaster))
                    return;

                Downloader = new Downloader(Options);

                try
                {
                }
                catch (TaskCanceledException)
                {
                }
                catch (Exception e)
                {
                    Logger.DownloaderLog("Unknown error, please retry " + e.Message);
                }
            }
            else
            {
                Options.TemporaryFile = $"{Path.GetFileNameWithoutExtension(Options.FileName)}-tmp.ts";

                try
                {
                    await Recorder.RecordAsync().ConfigureAwait(false);

                    while (Recorder.IsRecording && !_exitRequested) { } // ...?

                    await Recorder.DisconnectAsync().ConfigureAwait(false);
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