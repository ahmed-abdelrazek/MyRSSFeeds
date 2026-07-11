using MyRSSFeeds.Core.Data;
using MyRSSFeeds.Core.Services;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Quic;
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

        // Used to retry requests that bot-protected sites answer with 403 Forbidden
        internal const string FallbackBrowserUserAgent = LiteDbContext.ChromeWindowsAgentString;

        private const int MaxManualRedirects = 5;

        private static readonly HttpClient httpClient = new HttpClient(new HttpClientHandler
        {
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 10
        });

        // hosts where a QUIC handshake already failed - skip HTTP/3 for them so
        // every fetch doesn't stall for the whole handshake timeout again
        private static readonly ConcurrentDictionary<string, byte> quicFailedHosts = new ConcurrentDictionary<string, byte>();

        private readonly UserAgentService _userAgentService;

        public RssRequest(UserAgentService userAgentService)
        {
            _userAgentService = userAgentService;
        }

        /// <summary>
        /// Gets source data from a website as string
        /// </summary>
        /// <param name="url">Vaild url as string</param>
        /// <param name="useBrowserUserAgent">true when the source is known to require a browser user agent</param>
        /// <returns>Task for the webpage source (hopefully a xml one) and whether the
        /// fallback browser user agent is what made the request succeed</returns>
        public async Task<(string Feed, bool UsedBrowserUserAgent)> GetFeedAsStringAsync(string url, CancellationToken cancellationToken, bool useBrowserUserAgent = false)
        {
            return await GetFeedAsStringAsync(new Uri(url), cancellationToken, useBrowserUserAgent);
        }

        /// <summary>
        /// Gets source data from website as string
        /// </summary>
        /// <param name="url">Vaild url as Uri</param>
        /// <param name="useBrowserUserAgent">true when the source is known to require a browser user agent</param>
        /// <returns>Task for the webpage source (hopefully a xml one) and whether the
        /// fallback browser user agent is what made the request succeed</returns>
        public async Task<(string Feed, bool UsedBrowserUserAgent)> GetFeedAsStringAsync(Uri url, CancellationToken cancellationToken, bool useBrowserUserAgent = false)
        {
            string userAgent = useBrowserUserAgent ? FallbackBrowserUserAgent : BrowserUserAgent;
            HttpResponseMessage response = await SendAsync(url, userAgent, useBrowserUserAgent, cancellationToken);
            bool usedBrowserUserAgent = useBrowserUserAgent;

            // bot-protected sites reject non-browser agents with 403 Forbidden - retry
            // once pretending to be a browser so the feed still loads. Also retry when
            // the refused attempt went over TCP: its 403 response advertises HTTP/3
            // via Alt-Svc, and some protections only accept the QUIC connection
            if (response.StatusCode == HttpStatusCode.Forbidden
                && (userAgent != FallbackBrowserUserAgent || response.Version.Major < 3))
            {
                response.Dispose();
                response = await SendAsync(url, FallbackBrowserUserAgent, preferHttp3: true, cancellationToken);
                usedBrowserUserAgent = response.IsSuccessStatusCode;
            }

            using (response)
            {
                return (await ReadFeedAsString(response), usedBrowserUserAgent);
            }
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
        private static async Task<HttpResponseMessage> SendAsync(Uri url, string userAgent, bool preferHttp3, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await SendOnceAsync(url, userAgent, preferHttp3, cancellationToken);

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
                response = await SendOnceAsync(url, userAgent, preferHttp3, cancellationToken);
            }

            return response;
        }

        /// <summary>
        /// Sends a single request. Networks that block UDP 443 (and some CDNs) fail the
        /// QUIC handshake with an exception instead of downgrading, so fall back to TCP
        /// ourselves and skip HTTP/3 for that host from then on
        /// </summary>
        private static async Task<HttpResponseMessage> SendOnceAsync(Uri url, string userAgent, bool preferHttp3, CancellationToken cancellationToken)
        {
            preferHttp3 = preferHttp3 && !quicFailedHosts.ContainsKey(url.Host);
            try
            {
                return await httpClient.SendAsync(BuildRequest(url, userAgent, preferHttp3), cancellationToken);
            }
            catch (HttpRequestException ex) when (preferHttp3 && ex.InnerException is QuicException)
            {
                quicFailedHosts.TryAdd(url.Host, 0);
                return await httpClient.SendAsync(BuildRequest(url, userAgent, preferHttp3: false), cancellationToken);
            }
        }

        private static HttpRequestMessage BuildRequest(Uri url, string userAgent, bool preferHttp3)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (preferHttp3)
            {
                // some bot protections block .NET's TCP TLS fingerprint but accept
                // QUIC, so the browser user agent fallback asks for HTTP/3 first
                request.Version = HttpVersion.Version30;
                request.VersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
            }
            // strict "application/xml" alone makes some servers answer 406 Not Acceptable
            request.Headers.Add("Accept", "application/rss+xml, application/atom+xml, application/xml, text/xml, */*;q=0.8");
            // the agent can be user-provided (or empty when system info was missing at
            // first-run seeding) so add it leniently instead of throwing FormatException
            if (!string.IsNullOrWhiteSpace(userAgent))
            {
                request.Headers.TryAddWithoutValidation("User-Agent", userAgent);
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
