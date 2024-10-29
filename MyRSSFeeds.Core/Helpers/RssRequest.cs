using LiteDB;
using MyRSSFeeds.Core.Data;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MyRSSFeeds.Core.Helpers
{
    /// <summary>
    /// Handles the web requests to the websites by using HttpClient to get their rss data
    /// </summary>
    public class RssRequest
    {
        public static string BrowserUserAgent { get; set; } = "MyRSSFeeds/1.7 (Windows NT 10.0; X64)";

        private static readonly HttpClient httpClient;

        static RssRequest()
        {
            httpClient ??= new HttpClient();
        }

        /// <summary>
        /// Gets source data from a website as string
        /// </summary>
        /// <param name="url">Vaild url as string</param>
        /// <returns>Task string for the webpage source hopefully a xml one</returns>
        public static async Task<string> GetFeedAsStringAsync(string url, CancellationToken cancellationToken)
        {
            AddHttpClientHeaders();
            HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken);
            return await ReadFeedAsString(response);
        }

        /// <summary>
        /// Gets source data from website as string
        /// </summary>
        /// <param name="url">Vaild url as Uri</param>        
        /// <returns>Task string for the webpage source hopefully a xml one</returns>
        public static async Task<string> GetFeedAsStringAsync(Uri url, CancellationToken cancellationToken)
        {
            AddHttpClientHeaders();
            HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken);
            return await ReadFeedAsString(response);
        }

        public static async Task SetCustomUserAgentAsync()
        {
            await Task.Run(() =>
            {
                var agents = new Services.UserAgentService(new LiteDatabase(LiteDbContext.ConnectionString)).GetAgentData(x => x.IsUsed);
                var CurrentAgent = agents.FirstOrDefault();
                if (CurrentAgent != null)
                {
                    if (!string.IsNullOrEmpty(CurrentAgent.AgentString))
                    {
                        BrowserUserAgent = CurrentAgent.AgentString;
                    }
                }
            });
        }

        private static void AddHttpClientHeaders()
        {
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/xml");
            httpClient.DefaultRequestHeaders.Add("User-Agent", BrowserUserAgent);
        }

        private static async Task<string> ReadFeedAsString(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();
            var feedXML = await response.Content.ReadAsStringAsync();
            return feedXML.TrimStart();
        }
    }
}
