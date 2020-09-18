namespace Devil7.Utils.Automation.NineAnimeDownloader.Models
{
    public class Episode
    {
        public string Id { get; }
        public string URL { get; }
        public string Name { get; }

        public Episode(string id, string url, string name)
        {
            this.Id = id;
            this.URL = url;
            this.Name = name;
        }
    }
}
