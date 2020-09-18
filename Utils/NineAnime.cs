using Devil7.Utils.Automation.NineAnimeDownloader.Models;
using AngleSharp;
using AngleSharp.Dom;
using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Devil7.Utils.Automation.NineAnimeDownloader.Utils
{
    public static class NineAnime
    {
        private static RestClient client;
        private static string mcloudKey;

        public static void SetDomain(string domain)
        {
            client = new RestClient(domain);

            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;
        }

        public static async Task<Server[]> GetServers(string animeId)
        {
            return await Task.Run(async () =>
            {
                IConfiguration config = Configuration.Default;
                IBrowsingContext context = BrowsingContext.New(config);

                List<Server> servers = new List<Server>();

                RestRequest request = new RestRequest(APINodes.GET_EPISODES(animeId));
                RestResponse response = (RestResponse)client.Execute(request);
                if (response.IsSuccessful)
                {
                    HTMLJSONResponse jsonResponse = JsonConvert.DeserializeObject<HTMLJSONResponse>(response.Content);
                    if (jsonResponse != null)
                    {
                        IDocument document = await context.OpenAsync(req => req.Content(jsonResponse.html));
                        IHtmlCollection<IElement> serverElements = document.QuerySelectorAll(".widget.servers .widget-title .tabs .tab");

                        foreach (IElement serverElement in serverElements)
                        {
                            Server server = new Server(int.Parse(serverElement.GetAttribute("data-name")), serverElement.TextContent);

                            IHtmlCollection<IElement> episodeElements = document.QuerySelectorAll(string.Format(".widget.servers .widget-body .server[data-id=\"{0}\"] a", server.Id));
                            foreach (IElement episodeElement in episodeElements)
                            {
                                server.Episodes.Add(new Episode(episodeElement.GetAttribute("data-id"), episodeElement.GetAttribute("href"), episodeElement.TextContent));
                            }

                            servers.Add(server);
                        }
                    }
                }

                RestRequest mcloudKeyRequest = new RestRequest("https://mcloud.to/key");
                mcloudKeyRequest.AddHeader("Referer", client.BaseUrl.ToString());
                RestResponse mcloudKeyResponse = (RestResponse)client.Execute(mcloudKeyRequest);
                if (mcloudKeyResponse.IsSuccessful)
                {
                    Match match = Regex.Match(mcloudKeyResponse.Content, Patterns.REGEX_MCLOUDKEY);
                    if (match.Success)
                    {
                        mcloudKey = match.Groups["key"].Value;
                    }
                }

                return servers.ToArray();
            });
        }

        public static async Task<string> GetVideoUrl(int serverId, string episodeId)
        {
            IConfiguration config = Configuration.Default;
            IBrowsingContext context = BrowsingContext.New(config);

            return await Task.Run(async () =>
            {
                RestRequest infoRequest = new RestRequest(APINodes.GET_EPISODEINFO(serverId, episodeId, mcloudKey));
                RestResponse infoResponse = (RestResponse)client.Execute(infoRequest);
                if (infoResponse.IsSuccessful)
                {
                    EpisodeInfoResponse info = JsonConvert.DeserializeObject<EpisodeInfoResponse>(infoResponse.Content);
                    if (info != null)
                    {
                        RestRequest request = new RestRequest(info.target);
                        RestResponse response = (RestResponse)client.Execute(request);
                        if (response.IsSuccessful)
                        {
                            switch (serverId)
                            {
                                case 28: // MyCloud 
                                    {
                                        return string.Empty; // TODO
                                    }
                                case 35: // Mp4upload 
                                    {
                                        return string.Empty; // TODO
                                    }
                                case 40: // Streamtape
                                    {
                                        IDocument document = await context.OpenAsync(req => req.Content(response.Content));
                                        IElement videoElement = document.QuerySelector("#videolink");
                                        return string.Format("https:{0}", videoElement.TextContent);
                                    }
                            }
                        }
                    }
                }

                return string.Empty;
            });
        }
    }
}
