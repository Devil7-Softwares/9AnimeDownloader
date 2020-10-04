using AngleSharp;
using AngleSharp.Dom;
using Devil7.Utils.Automation.NineAnimeDownloader.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

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

        public static string Decrypt(string value)
        {
            string keyString = value.Substring(0, 9);
            string encryptedEncodedString = value.Substring(9);

            int calculatedIndex = 0;
            int index = 0;

            // #region Decoding
            if ((encryptedEncodedString = Regex.Replace(encryptedEncodedString, "[ \t\n\f\r]", "")).Length % 4 == 0)
            {
                encryptedEncodedString = Regex.Replace(encryptedEncodedString, "==?$", "");
                if (encryptedEncodedString.Length % 4 == 1 || new Regex("[^+/0-9A-Za-z]").IsMatch(encryptedEncodedString))
                {
                    return null;
                }
            }

            string encryptedDecodedString = "";

            int offset = 0;
            for (index = 0; index < encryptedEncodedString.Length; index++)
            {
                offset <<= 6;

                int charIndex = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".IndexOf(encryptedEncodedString[index]);
                offset |= charIndex < 0 ? 0 : charIndex;

                if (((index + 1) % 4) == 0)
                {
                    encryptedDecodedString += Convert.ToChar((16711680 & offset) >> 16);
                    encryptedDecodedString += Convert.ToChar((65280 & offset) >> 8);
                    encryptedDecodedString += Convert.ToChar((255 & offset));
                    offset = 0;
                }
                if (index == encryptedEncodedString.Length - 1)
                {
                    switch (index % 4)
                    {
                        case 1:
                            offset >>= 4;
                            encryptedDecodedString += Convert.ToChar(offset);
                            break;
                        case 2:
                            offset >>= 2;
                            encryptedDecodedString += Convert.ToChar((65280 & offset) >> 8);
                            encryptedDecodedString += Convert.ToChar(((255) & offset));
                            break;
                    }
                }
            }

            try
            {
                encryptedDecodedString = HttpUtility.UrlDecode(encryptedDecodedString);
            }
            catch (Exception err) { }
            // #endregion

            // #region Decryption
            Dictionary<int, int> tmpArray = Enumerable.Range(0, 256).Select(i => new { Key = i, Value = i }).ToDictionary(x => x.Key, x => x.Value);

            for (index = 0; index < 256; index++)
            {
                calculatedIndex = ((calculatedIndex + tmpArray[index] + Convert.ToUInt16(keyString.ToCharArray()[index % keyString.Length])) % 256);

                int tmp = tmpArray[index];
                tmpArray[index] = tmpArray[calculatedIndex];
                tmpArray[calculatedIndex] = tmp;
            }

            string decryptedDecodedString = "";
            for (int s = calculatedIndex = index = 0; s < encryptedDecodedString.Length; s++)
            {
                calculatedIndex = (calculatedIndex + tmpArray[index = (index + 1) % 256]) % 256;

                int tmp = tmpArray[index];
                tmpArray[index] = tmpArray[calculatedIndex];
                tmpArray[calculatedIndex] = tmp;

                decryptedDecodedString += Convert.ToChar(Convert.ToUInt16(encryptedDecodedString.ToCharArray()[s]) ^ tmpArray[(tmpArray[index] + tmpArray[calculatedIndex]) % 256]);
            }
            // #endregion

            return decryptedDecodedString;
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
                    IDocument document = await context.OpenAsync(req => req.Content(response.Content));
                    IHtmlCollection<IElement> serverElements = document.QuerySelectorAll("section div.head .tabs.servers span");

                    foreach (IElement serverElement in serverElements)
                    {
                        Server server = new Server(int.Parse(serverElement.GetAttribute("data-id")), serverElement.TextContent);

                        IHtmlCollection<IElement> episodeElements = document.QuerySelectorAll("section div.body a[data-sources]");
                        foreach (IElement episodeElement in episodeElements)
                        {
                            JObject dataSources = JObject.Parse(episodeElement.GetAttribute("data-sources"));
                            server.Episodes.Add(new Episode(dataSources[server.Id.ToString()].ToString(), episodeElement.GetAttribute("href"), episodeElement.TextContent));
                        }

                        servers.Add(server);
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
                        string decryptedUrl = Decrypt(info.url);
                        RestRequest request = new RestRequest(decryptedUrl);
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
