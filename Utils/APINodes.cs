using System;
using System.Collections.Generic;
using System.Text;

namespace _9Anime_Downloader.Utils
{
    public static class APINodes
    {
        public static string GET_EPISODES(string animeId) => string.Format("/ajax/film/servers?id={0}&ts={1}", animeId, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
    }
}
