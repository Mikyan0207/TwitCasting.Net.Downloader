namespace TwitCasting.Net.Downloader.Internal
{
    internal static class Endpoints
    {
        public static string DefaultUrl => "https://twitcasting.tv";

        public static string StreamServer => DefaultUrl + "/streamserver.php";

        public static string Download => "http://dl01.twitcasting.tv/{user}/download/{liveId}?dl=1";
    }
}