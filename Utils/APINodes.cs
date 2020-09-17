using System;

namespace _9Anime_Downloader.Utils
{
    public static class APINodes
    {
        public static string GET_EPISODES(string animeId) => string.Format("/ajax/film/servers?id={0}&ts={1}", animeId, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        public static string GET_EPISODEINFO(int serverId, string episodeId, string mcloudKey) => string.Format("/ajax/episode/info?id={0}&server={1}&mcloud={2}&ts={3}", episodeId, serverId, mcloudKey, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
    }
}
