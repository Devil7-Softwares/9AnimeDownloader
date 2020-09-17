using System;
using System.Collections.Generic;
using System.Text;

namespace _9Anime_Downloader.Models
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
