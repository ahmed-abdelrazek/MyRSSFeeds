using MyRSSFeeds.Core.Services;
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
        public string BrowserUserAgent { get; set; } = "MyRSSFeeds/1.7 (Windows NT 10.0; X64)";

        private const int MaxManualRedirects = 5;

        private static readonly HttpClient httpClient = new HttpClient(new HttpClientHandler
        {
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 10
        });

        private readonly UserAgentService _userAgentService;

        public RssRequest(UserAgentService userAgentService)
        {
            _userAgentService = userAgentService;
        }

        /// <summary>
        /// Gets source data from a website as string
        /// </summary>
        /// <param name="url">Vaild url as string</param>
        /// <returns>Task string for the webpage source hopefully a xml one</returns>
        public async Task<string> GetFeedAsStringAsync(string url, CancellationToken cancellationToken)
        {
            return await GetFeedAsStringAsync(new Uri(url), cancellationToken);
        }

        /// <summary>
        /// Gets source data from website as string
        /// </summary>
        /// <param name="url">Vaild url as Uri</param>
        /// <returns>Task string for the webpage source hopefully a xml one</returns>
        public async Task<string> GetFeedAsStringAsync(Uri url, CancellationToken cancellationToken)
        {
            using HttpResponseMessage response = await SendAsync(url, cancellationToken);
            return await ReadFeedAsString(response);
        }

        public async Task SetCustomUserAgentAsync()
        {
            await Task.Run(() =>
            {
                var currentAgent = _userAgentService.GetAgentData(x => x.IsUsed).FirstOrDefault();
                if (!string.IsNullOrEmpty(currentAgent?.AgentString))
                {
                    BrowserUserAgent = currentAgent.AgentString;
                }
            });
        }

        /// <summary>
        /// Sends the request, following redirects HttpClient refuses to follow on
        /// its own (e.g. https to http downgrades), so feeds that moved still load
        /// </summary>
        private async Task<HttpResponseMessage> SendAsync(Uri url, CancellationToken cancellationToken)
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

                // Never downgrade to plain http when the request started on https:
                // besides the security loss, ISPs can hijack unencrypted requests
                // (e.g. sites 301-ing https -> http land on ISP redirect pages)
                if (url.Scheme == Uri.UriSchemeHttps && location.Scheme == Uri.UriSchemeHttp)
                {
                    location = new UriBuilder(location)
                    {
                        Scheme = Uri.UriSchemeHttps,
                        Port = location.IsDefaultPort ? 443 : location.Port
                    }.Uri;
                }

                url = location;
                response.Dispose();
                response = await httpClient.SendAsync(BuildRequest(url), cancellationToken);
            }

            return response;
        }

        private HttpRequestMessage BuildRequest(Uri url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            // strict "application/xml" alone makes some servers answer 406 Not Acceptable
            request.Headers.Add("Accept", "application/rss+xml, application/atom+xml, application/xml, text/xml, */*;q=0.8");
            // the agent can be user-provided (or empty when system info was missing at
            // first-run seeding) so add it leniently instead of throwing FormatException
            if (!string.IsNullOrWhiteSpace(BrowserUserAgent))
            {
                request.Headers.TryAddWithoutValidation("User-Agent", BrowserUserAgent);
            }
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
