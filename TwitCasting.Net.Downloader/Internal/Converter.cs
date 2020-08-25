using System;
using System.Linq;
using System.Threading.Tasks;
using TwitCasting.Net.Downloader.Common;
using Xabe.FFmpeg;
using static Xabe.FFmpeg.FFmpeg;

namespace TwitCasting.Net.Downloader.Internal
{
    internal static class Converter
    {
        public static async Task Convert(Options options)
        {
            var mediaInfo = await GetMediaInfo(options.TemporaryFile + ".mp4");

            IStream videoStream = mediaInfo.VideoStreams
                .FirstOrDefault()
                ?.SetSize(VideoSize.Hd1080)
                .SetCodec(VideoCodec.hevc)
                .SetFramerate(60);

            IStream audioStream = mediaInfo.AudioStreams
                .FirstOrDefault()
                ?.SetCodec(AudioCodec.aac);

            var conv = FFmpeg.Conversions.New()
                .AddStream(videoStream, audioStream)
                .SetOutput(options.FileName + ".mp4")
                .SetOutputFormat(Format.mp4)
                .UseShortest(true)
                .SetOverwriteOutput(true);

            conv.OnProgress += (sender, args) =>
            {
                var percent = (int)(Math.Round(args.Duration.TotalSeconds / args.TotalLength.TotalSeconds, 2) * 100);
                Logger.UpdateLine("{2} " + $"{args.Duration} / {args.TotalLength} ({percent}%)");
            };

            await conv.Start().ConfigureAwait(false);
        }

        public static async Task Convert(string input, string output)
        {
            var mediaInfo = await GetMediaInfo(input);

            IStream videoStream = mediaInfo.VideoStreams
                .FirstOrDefault()
                ?.SetSize(VideoSize.Hd1080)
                .SetCodec(VideoCodec.hevc)
                .SetFramerate(60);

            IStream audioStream = mediaInfo.AudioStreams
                .FirstOrDefault()
                ?.SetCodec(AudioCodec.aac);

            var conv = FFmpeg.Conversions.New()
                .AddStream(videoStream, audioStream)
                .SetOutput(output)
                .SetOutputFormat(Format.mp4)
                .UseShortest(true)
                .SetOverwriteOutput(true);

            conv.OnProgress += (sender, args) =>
            {
                var percent = (int)(Math.Round(args.Duration.TotalSeconds / args.TotalLength.TotalSeconds, 2) * 100);
                Logger.UpdateLine("{2} " + $"{args.Duration} / {args.TotalLength} ({percent}%)");
            };

            await conv.Start().ConfigureAwait(false);
        }

        // ReSharper disable once InconsistentNaming
        public static async Task FFMpegDownload(string url, string output)
        {
            var conversion = FFmpeg.Conversions
                .New()
                .AddParameter("-max_muxing_queue_size 400")
                .AddStream(GetMediaInfo(url).GetAwaiter().GetResult().Streams)
                .SetOutputFormat(Format.matroska)
                .SetOutput($"{output}.mkv")
                .SetOverwriteOutput(true);

            conversion.OnProgress += (sender, args) =>
            {
                var percent = (int)(Math.Round(args.Duration.TotalSeconds / args.TotalLength.TotalSeconds, 2) * 100);
                Logger.UpdateLine("{2} " + $"{args.Duration} / {args.TotalLength} ({percent}%)");
            };

#if DEBUG
            conversion.OnDataReceived += (sender, args) =>
            {
                Logger.ConverterLog(args.Data);
            };
#endif

            await conversion.Start().ConfigureAwait(false);
            await Convert($"{output}.mkv", $"{output}.mp4");
        }
    }
}