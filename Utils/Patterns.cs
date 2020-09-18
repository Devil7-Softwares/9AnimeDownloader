namespace Devil7.Utils.Automation.NineAnimeDownloader.Utils
{
    public static class Patterns
    {
        public const string REGEX_MAINURL = @"(?<domain>http[s]?:\/\/.*?\..*?\..*?\/)\watch\/.*(\.(?<animeId>.*?)\/).*";
        public const string REGEX_MCLOUDKEY = @"window.mcloudKey='(?<key>.*)';";
    }
}
