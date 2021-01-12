using Colorful;
using System.Drawing;
using Console = Colorful.Console;

namespace TwitCasting.Net.Downloader.Internal
{
    public static class Logger
    {
        private static readonly Formatter[] Formatters = {
            new Formatter("[Downloader]", Color.Crimson),
            new Formatter("[Recorder]", Color.Crimson),
            new Formatter("[WebSocket]", Color.Magenta),
            new Formatter("[Converter]", Color.Green)
        };

        public static void Log(string message)
            => Console.WriteLineFormatted(message, Color.White, Formatters);

        public static void UpdateLine(string message)
        {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.WriteLineFormatted(message, Color.White, Formatters);
        }

        public static void WebSocketLog(string message)
        {
            Console.WriteFormatted("{2} ", Color.White, Formatters);
            Log(message);
        }

        public static void DownloaderLog(string message)
        {
            Console.WriteFormatted("{0} ", Color.White, Formatters);
            Log(message);
        }

        public static void ConverterLog(string message)
        {
            Console.WriteFormatted("{3} ", Color.White, Formatters);
            Log(message);
        }
    }
}