using Devil7.Utils.Automation.NineAnimeDownloader.Models;
using Devil7.Utils.Automation.NineAnimeDownloader.Utils;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Devil7.Utils.Automation.NineAnimeDownloader
{
    class Program
    {
        private static string AnimeId = "";
        private static Server SelectedServer = null;
        private static string[] EpisodesToDownload = null;

        static void Main(string[] args)
        {
            while (true)
            {
                Console.Write("Enter URL 9Anime Website URL: ");
                string tmpURL = Console.ReadLine();
                Match urlMatch = Regex.Match(tmpURL, Patterns.REGEX_MAINURL);
                if (urlMatch.Success)
                {
                    NineAnime.SetDomain(urlMatch.Groups["domain"].Value);
                    AnimeId = urlMatch.Groups["animeId"].Value;
                    break;
                }
                else
                {
                    Console.Error.Write("ERROR: Invalid URL. ");
                    TryAgainOrQuit();
                }
            }

            Console.Clear();
            Console.WriteLine("Fetching servers and episode details. Please wait...!");

            Server[] servers = NineAnime.GetServers(AnimeId).Result;
            if (servers == null || servers.Length == 0)
            {
                Console.Error.Write("No Servers Found for Given URL! ");
                Quit();
            }

            Console.Clear();
            while (true)
            {
                Console.WriteLine("Available Servers:");
                for (int i = 0; i < servers.Length; i++)
                {
                    Server server = servers[i];
                    Console.WriteLine("\t{0}. {1}\t({2} Episode{3})", i + 1, server.Name, server.Episodes.Count, server.Episodes.Count > 1 ? "s" : "");
                }
                try
                {
                    Console.Write("\nSelect Server: ");
                    int choice = int.Parse(Console.ReadLine());
                    if (choice > 0 && choice <= servers.Length)
                    {
                        SelectedServer = servers[choice - 1];
                        break;
                    }
                    else
                    {
                        Console.Error.WriteLine("Choice is out of range! ");
                        TryAgainOrQuit();
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.Write("Invalid choice! ");
                    TryAgainOrQuit();
                }
            }

            string[] availableEpisodes = SelectedServer.Episodes.Select(episode => episode.Name).ToArray();

            Console.Clear();
            while (true)
            {
                try
                {
                    Console.WriteLine("Selected Server: {0} ({1} Episode{2})", SelectedServer.Name, SelectedServer.Episodes.Count, SelectedServer.Episodes.Count > 1 ? "s" : "");
                    Console.WriteLine("Available Episodes:\n{0}", Common.ArrayToRangeString(availableEpisodes));
                    Console.Write("\nEnter Episodes to Download (Comma Separated Values & Ranges Allowed e.g. 1, 3, 5-10): ");

                    string episodesToDownloadString = Console.ReadLine();
                    string[] episodesToDownload = Common.SingleLineStringToStrings(episodesToDownloadString, availableEpisodes);
                    if (episodesToDownload.Length == 0)
                    {
                        Console.Error.Write("No Episodes to Download! ");
                        TryAgainOrQuit();
                    }
                    else
                    {
                        EpisodesToDownload = episodesToDownload;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.Write("Invalid Input! ");
                    TryAgainOrQuit();
                }
            }

            Console.Clear();
            while (true)
            {
                Console.WriteLine("Selected Server: {0} ({1} Episode{2})", SelectedServer.Name, SelectedServer.Episodes.Count, SelectedServer.Episodes.Count > 1 ? "s" : "");
                Console.WriteLine("Selcted Episodes:\n{0}", Common.ArrayToRangeString(EpisodesToDownload));
                Console.Write("\nDo you want to continue downloading above {0} episodes? [Y/n]? ", EpisodesToDownload.Length);
                ConsoleKeyInfo keyInfo = Console.ReadKey();

                if (keyInfo.Key == ConsoleKey.Y || keyInfo.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else
                {
                    Environment.Exit(0);
                }
            }

            Console.Clear();

            Console.ReadLine();
        }

        static void TryAgainOrQuit()
        {
            Console.WriteLine("Press Enter to Try Again... Or Any Other Key to Quit!");
            ConsoleKeyInfo key = Console.ReadKey();
            if (key.Key == ConsoleKey.Enter)
            {
                Console.Clear();
            }
            else
            {
                Environment.Exit(0);
            }
        }

        static void Quit()
        {
            Console.WriteLine("Press Enter to Exit...");
            Console.ReadLine();
            Environment.Exit(0);
        }
    }
}
