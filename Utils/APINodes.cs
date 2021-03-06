﻿using System;

namespace Devil7.Utils.Automation.NineAnimeDownloader.Utils
{
    public static class APINodes
    {
        public static string GET_EPISODES(string animeId) => string.Format("/ajax/anime/servers?id={0}&ts={1}", animeId, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        public static string GET_EPISODEINFO(int serverId, string episodeId, string mcloudKey) => string.Format("/ajax/anime/episode?id={0}&server={1}&mcloud={2}&ts={3}", episodeId, serverId, mcloudKey, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
    }
}
