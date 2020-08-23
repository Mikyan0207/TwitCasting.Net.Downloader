using System;
using System.Linq;
using System.Threading.Tasks;
using Xabe.FFmpeg;
using static Xabe.FFmpeg.FFmpeg;

namespace TwitCasting.Net.Downloader.Internal
{
    internal static class Converter
    {
        public static async Task Convert(Options options)
        {
            var mediaInfo = await GetMediaInfo(options.TemporaryFile);

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

            await conv.Start();
        }
    }
}