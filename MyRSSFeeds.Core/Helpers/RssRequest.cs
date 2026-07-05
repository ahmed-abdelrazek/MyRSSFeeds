using LiteDB;
using MyRSSFeeds.Core.Data;
using System;
using System.Linq;
using System.Net;
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

        private const int MaxManualRedirects = 5;

        private static readonly HttpClient httpClient = new HttpClient(new HttpClientHandler
        {
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 10
        });

        /// <summary>
        /// Gets source data from a website as string
        /// </summary>
        /// <param name="url">Vaild url as string</param>
        /// <returns>Task string for the webpage source hopefully a xml one</returns>
        public static async Task<string> GetFeedAsStringAsync(string url, CancellationToken cancellationToken)
        {
            return await GetFeedAsStringAsync(new Uri(url), cancellationToken);
        }

        /// <summary>
        /// Gets source data from website as string
        /// </summary>
        /// <param name="url">Vaild url as Uri</param>
        /// <returns>Task string for the webpage source hopefully a xml one</returns>
        public static async Task<string> GetFeedAsStringAsync(Uri url, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await SendAsync(url, cancellationToken);
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

        /// <summary>
        /// Sends the request, following redirects HttpClient refuses to follow on
        /// its own (e.g. https to http downgrades), so feeds that moved still load
        /// </summary>
        private static async Task<HttpResponseMessage> SendAsync(Uri url, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await httpClient.SendAsync(BuildRequest(url), cancellationToken);

            for (int hop = 0; hop < MaxManualRedirects && IsRedirect(response.StatusCode); hop++)
            {
                Uri location = response.Headers.Location;
                if (location == null)
                {
                    break;
                }

                if (!location.IsAbsoluteUri)
                {
                    location = new Uri(url, location);
                }

                url = location;
                response = await httpClient.SendAsync(BuildRequest(url), cancellationToken);
            }

            return response;
        }

        private static HttpRequestMessage BuildRequest(Uri url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            // strict "application/xml" alone makes some servers answer 406 Not Acceptable
            request.Headers.Add("Accept", "application/rss+xml, application/atom+xml, application/xml, text/xml, */*;q=0.8");
            request.Headers.Add("User-Agent", BrowserUserAgent);
            return request;
        }

        private static bool IsRedirect(HttpStatusCode statusCode)
        {
            return statusCode is HttpStatusCode.MovedPermanently // 301
                or HttpStatusCode.Redirect // 302
                or HttpStatusCode.SeeOther // 303
                or HttpStatusCode.TemporaryRedirect // 307
                or HttpStatusCode.PermanentRedirect; // 308
        }

        private static async Task<string> ReadFeedAsString(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();
            var feedXML = await response.Content.ReadAsStringAsync();
            return feedXML.TrimStart();
        }
    }
}
