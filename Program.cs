using Devil7.Utils.Automation.NineAnimeDownloader.Models;
using Devil7.Utils.Automation.NineAnimeDownloader.Utils;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Devil7.Utils.Automation.NineAnimeDownloader
{
    class Program
    {
        private static string AnimeId = "";
        private static Server SelectedServer = null;
        private static string[] EpisodesToDownloadArray = null;

        private static int ConcurrentJobs = 1;
        private static readonly Queue<Episode> EpisodesToDownload = new Queue<Episode>();
        private static readonly List<DownloadJob> DownloadJobs = new List<DownloadJob>();

        private static ProgressBar MainProgressBar;

        public static int TotalDownloads = 0;
        public static int CompletedDownloads = 0;
        public static int FailedDownloads = 0;
        public static List<string> ErrorMessages = new List<string>();

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
                        EpisodesToDownloadArray = episodesToDownload;
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
                Console.WriteLine("Selcted Episodes:\n{0}", Common.ArrayToRangeString(EpisodesToDownloadArray));
                Console.Write("\nDo you want to continue downloading above {0} episodes? [Y/n]? ", EpisodesToDownloadArray.Length);
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
            while (true)
            {
                try
                {
                    Console.Write("Enter Number of Concurrent Jobs:");
                    ConcurrentJobs = int.Parse(Console.ReadLine());

                    if (ConcurrentJobs < 1)
                        ConcurrentJobs = 1;

                    break;
                }
                catch (Exception ex)
                {
                    Console.Error.Write("Invalid Input! ");
                    TryAgainOrQuit();
                }
            }

            foreach (string episodeName in EpisodesToDownloadArray)
            {
                Episode episode = SelectedServer.Episodes.Find(episode => episode.Name == episodeName);
                if (episode != null)
                    EpisodesToDownload.Enqueue(episode);
            }

            TotalDownloads = EpisodesToDownload.Count;

            ProgressBarOptions progressBarOptions = new ProgressBarOptions()
            {
                ForegroundColor = ConsoleColor.Green,
                ProgressBarOnBottom = true
            };
            MainProgressBar = new ProgressBar((int)(TotalDownloads * 100), string.Format("Downloading {0} Episodes...", TotalDownloads), progressBarOptions);

            for (int i = 0; i < ConcurrentJobs; i++)
            {
                AddJob();
            }

            System.Timers.Timer progressUpdateTimer = new System.Timers.Timer(500)
            {
                AutoReset = true,
            };
            progressUpdateTimer.Elapsed += ProgressUpdateTimer_Elapsed;
            progressUpdateTimer.Start();

            IEnumerable<Task> Tasks = GetIncompleteTasks();
            while (Tasks.Count() > 0)
            {
                Task.WhenAll(Tasks).Wait();
                Tasks = GetIncompleteTasks();
            }

            Console.Clear();
            Console.WriteLine("=============================================");
            Console.WriteLine("             All Tasks Complted!             ");
            Console.WriteLine("=============================================");
            Console.WriteLine(" - Total Downloads\t: {0}", TotalDownloads);
            Console.WriteLine(" - Completed Downloads\t: {0}", CompletedDownloads);
            Console.WriteLine(" - Failed Downloads\t: {0}", FailedDownloads);
            if (ErrorMessages.Count > 0)
            {
                Console.Error.WriteLine(" - Error Messages\t: {0}", ErrorMessages[0]);
                for (var i = 1; i < ErrorMessages.Count; i++)
                    Console.Error.WriteLine("\t\t\t  * {0}", ErrorMessages[i]);
            }
            Console.WriteLine("=============================================");

            Console.Clear();
            NineAnime.GetVideoUrl(SelectedServer.Id, SelectedServer.Episodes.Find(episode => episode.Name == EpisodesToDownloadArray[0]).Id).Wait();


            Console.ReadLine();
        }

        private static void ProgressUpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            MainProgressBar.Tick(DownloadJobs.Sum((job) => job.Progress), string.Format("Downloaded {0} of {1}. {2} Failed!", CompletedDownloads, TotalDownloads, FailedDownloads));
        }

        private static IEnumerable<Task> GetIncompleteTasks()
        {
            return DownloadJobs.Select((job) => job.DownloadTask).Where((task) => !task.IsCompleted);
        }

        private static void AddJob()
        {
            if (EpisodesToDownload.Count > 0)
            {
                Episode episode = EpisodesToDownload.Dequeue();

                string downloadUrl = NineAnime.GetVideoUrl(SelectedServer.Id, episode.Id).Result;

                if (string.IsNullOrEmpty(downloadUrl))
                {
                    ErrorMessages.Add(string.Format("Episode {0}: Failed to Get Download URL!", episode.Name));
                    AddJob();
                }
                else
                {
                    DownloadJob job = new DownloadJob(episode.Name, downloadUrl, MainProgressBar);
                    job.Start();
                    DownloadJobs.Add(job);
                    job.DownloadTask.ContinueWith((Task task) => AddJob());
                }
            }
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
