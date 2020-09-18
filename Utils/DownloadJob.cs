using Devil7.Utils.Automation.NineAnimeDownloader.Models;
using ShellProgressBar;
using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

namespace Devil7.Utils.Automation.NineAnimeDownloader.Utils
{
    class DownloadJob : IDisposable
    {
        #region Variables
        private readonly ProgressBar ParentProgressBar;
        private ChildProgressBar ProgressBar;
        #endregion

        public string Url { get; }
        public string Episode { get; }
        public int Progress { get; private set; }
        public long DownloadedBytes { get; private set; }
        public long TotalBytes { get; private set; }
        public string DownloadedSize { get => BytesToString(this.DownloadedBytes); }
        public string TotalSize { get => BytesToString(this.TotalBytes); }
        public Task DownloadTask { get; }
        public TaskStatus Status { get => this.DownloadTask.Status; }

        public DownloadJob(string Episode, string Url, ProgressBar ParentProgressBar)
        {
            this.Episode = Episode;
            this.Url = Url;
            this.ParentProgressBar = ParentProgressBar;

            this.DownloadTask = new Task(Download);
        }

        public void Start()
        {
            if (DownloadTask.Status == TaskStatus.Created)
            {
                ProgressBarOptions progressBarOptions = new ProgressBarOptions()
                {
                    CollapseWhenFinished = true,
                    ForegroundColor = ConsoleColor.DarkCyan,
                    ProgressBarOnBottom = true
                };

                this.TotalBytes = GetFileSize(this.Url);
                this.ProgressBar = ParentProgressBar.Spawn((int)TotalBytes, string.Format("Episode {0}: Starting Download...", this.Episode), progressBarOptions);
                DownloadTask.Start();
            }
        }

        private void Download()
        {
            string filename = string.Format("Episode {0}.mp4", this.Episode);

            using WebClient wc = new WebClient();
            SpoofHeaders(wc.Headers);

            wc.DownloadProgressChanged += WebClient_DownloadProgressChanged;
            wc.DownloadFileCompleted += WebClient_DownloadFileCompleted;
            wc.DownloadFileTaskAsync(new Uri(this.Url), System.IO.Path.Combine(Environment.CurrentDirectory, filename)).Wait();
        }

        #region Event Handlers
        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.TotalBytes = e.TotalBytesToReceive;
            this.DownloadedBytes = e.BytesReceived;

            this.Progress = e.ProgressPercentage;

            this.ProgressBar.Tick((int)e.BytesReceived, string.Format("Episode {0}: Downloading... [{1}/{2}]", this.Episode, this.DownloadedSize, this.TotalSize));
        }

        private void WebClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.Progress = 100;

            if (e.Cancelled)
            {
                string errorMessage = string.Format("Episode {0}: Download Cancelled!", this.Episode);
                Program.FailedDownloads++;
                this.ProgressBar.Tick((int)this.TotalBytes, errorMessage);
                Program.ErrorMessages.Add(errorMessage);
            }
            else if (e.Error != null)
            {
                string errorMessage = string.Format("Episode {0}: Download Failed!", this.Episode);
                this.ProgressBar.Tick((int)this.TotalBytes, errorMessage);
                Program.ErrorMessages.Add(errorMessage);
            }
            else
            {
                Program.CompletedDownloads++;
                this.ProgressBar.Tick((int)this.TotalBytes, string.Format("Episode {0}: Download Completed!", this.Episode));
            }
        }
        #endregion

        #region Utils
        private void SpoofHeaders(WebHeaderCollection Headers)
        {
            Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.61 Safari/537.36");
        }

        private long GetFileSize(string url)
        {
            WebRequest webRequest = WebRequest.Create(new Uri(url));
            webRequest.Method = "HEAD";

            SpoofHeaders(webRequest.Headers);

            using WebResponse webResponse = webRequest.GetResponse();
            return long.Parse(webResponse.Headers.Get("Content-Length"));
        }

        private static string BytesToString(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (bytes >= 1024 && order < sizes.Length - 1)
            {
                order++;
                bytes /= 1024;
            }
            return String.Format("{0:0.00}{1}", bytes, sizes[order]);
        }
        #endregion

        #region IDisposable Implementation
        public void Dispose()
        {
            if (this.ProgressBar != null) this.ProgressBar.Dispose();
        }
        #endregion
    }
}
