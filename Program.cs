using _9Anime_Downloader.Utils;
using System;
using System.Text.RegularExpressions;

namespace _9Anime_Downloader
{
    class Program
    {
        private static string Domain = "";
        private static string AnimeId = "";

        static void Main(string[] args)
        {
            while (true)
            {
                Console.Write("\nEnter URL 9Anime Website URL: ");
                string tmpURL = Console.ReadLine();
                Match urlMatch = Regex.Match(tmpURL, Patterns.REGEX_MAINURL);
                if (urlMatch.Success)
                {
                    Domain = urlMatch.Groups["domain"].Value;
                    AnimeId = urlMatch.Groups["animeId"].Value;
                    break;
                }
                else
                {
                    Console.Error.WriteLine("ERROR: Invalid URL. Try again...");
                }
            }
        }
    }
}
