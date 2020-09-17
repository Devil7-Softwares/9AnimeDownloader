using System;
using System.Collections.Generic;
using System.Text;

namespace _9Anime_Downloader.Models
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
