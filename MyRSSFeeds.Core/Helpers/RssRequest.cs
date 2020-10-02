using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace MyRSSFeeds.Core.Helpers
{
    /// <summary>
    /// Handles the web requests to the websites by using HttpClient to get their rss data
    /// </summary>
    public class RssRequest
    {
        // MyRSSFeeds/1.0
        // Microsoft provided one "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)"
        public static string BrowserUserAgent { get; set; } = "MyRSSFeeds/1.0";

        private static readonly HttpClient HttpClient;

        static RssRequest()
        {
            HttpClient = new HttpClient();
            HttpClient.DefaultRequestHeaders.Add("User-Agent", BrowserUserAgent);
        }

        /// <summary>
        /// Gets source data from a website as string
        /// the bool is true if the url is vaild and the string is the source
        /// the bool is false if the url isn't and the string is empty
        /// </summary>
        /// <param name="url">Vaild url as string</param>
        /// <returns>Task (bool, string) as item1 and item2</returns>
        public static async Task<(bool, string)> GetFeedAsStringAsync(string url)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                try
                {
                    HttpResponseMessage response = await HttpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return (true, responseBody);
                }
                catch (HttpRequestException ex)
                {
                    Debug.WriteLine(ex);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
            return (false, string.Empty);
        }

        /// <summary>
        /// Gets source data from website as string
        /// the bool is true if the url is vaild and the string is the source
        /// the bool is false if the url isn't and the string is empty
        /// </summary>
        /// <param name="url">Vaild url as Uri</param>
        /// <returns>Task (bool, string) as item1 and item2</returns>
        public static async Task<(bool, string)> GetFeedAsStringAsync(Uri url)
        {
            if (url == null)
            {
                return (false, string.Empty);
            }
            return await GetFeedAsStringAsync(url.ToString());
        }
    }
}
