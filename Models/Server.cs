using System.Collections.Generic;

namespace Devil7.Utils.Automation.NineAnimeDownloader.Models
{
    public class Server
    {
        public int Id { get; }
        public string Name { get; }
        public List<Episode> Episodes { get; }

        public Server(int id, string name)
        {
            this.Id = id;
            this.Name = name;
            this.Episodes = new List<Episode>();
        }
    }
}
