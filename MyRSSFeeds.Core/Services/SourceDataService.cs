using LiteDB;
using MyRSSFeeds.Core.Contracts.Services;
using MyRSSFeeds.Core.Data;
using MyRSSFeeds.Core.Helpers;
using MyRSSFeeds.Core.Models;
using System;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;

namespace MyRSSFeeds.Core.Services
{
    public class SourceDataService : ISourceDataService
    {
        public async Task<ILiteQueryable<Source>> GetSourcesDataAsync()
        {
            return await Task.Run(() =>
            {
                using (var db = new LiteDatabase(LiteDbContext.DbConnectionString))
                {
                    return db.GetCollection<Source>(LiteDbContext.Sources).Query().OrderByDescending(x => x.Id);
                }
            });
        }

        public async Task<ILiteQueryable<Source>> GetSourcesDataWithFeedsAsync()
        {
            return await Task.Run(() =>
            {
                using (var db = new LiteDatabase(LiteDbContext.DbConnectionString))
                {
                    return db.GetCollection<Source>(LiteDbContext.Sources).Include(x => x.RSSs).Query();
                }
            });
        }

        /// <summary>
        /// Check if Source Exist by its base Uri
        /// </summary>
        /// <param name="source">the base Uri for the source</param>
        /// <returns>Task true if the source already exist</returns>
        public async Task<bool> SourceExistAsync(string source)
        {
            var link = new Uri(source);

            return await Task.Run(() =>
            {
                using var db = new LiteDatabase(LiteDbContext.DbConnectionString);
                return db.GetCollection<Source>(LiteDbContext.Sources).Exists(x => x.RssUrl == link);
            });
        }

        /// <summary>
        /// checks if a source works with last updated time and rss items count
        /// </summary>
        /// <param name="source">string for source rss url</param>
        /// <returns>Task (true if works, datetime offset for the last time website updated, int for rss items count)</returns>
        public async Task<(bool, DateTimeOffset, int)> IsSourceWorkingAsync(string source)
        {
            var feedString = await RssRequest.GetFeedAsStringAsync(source, new System.Threading.CancellationToken());

            using (XmlReader xmlReader = XmlReader.Create(new StringReader(feedString)))
            {
                SyndicationFeed feed = SyndicationFeed.Load(xmlReader);
                var lastUpdatedTime = feed.LastUpdatedTime;
                var rssItemsCount = feed.Items.Count();

                if (lastUpdatedTime.Year < 2020)
                {
                    if (rssItemsCount > 0)
                    {
                        var latestDateItem = feed.Items.OrderByDescending(x => x.PublishDate).FirstOrDefault();

                        if (latestDateItem is null)
                        {
                            latestDateItem = feed.Items.OrderByDescending(x => x.LastUpdatedTime).FirstOrDefault();
                            lastUpdatedTime = DateTimeOffset.Now;
                        }
                        else
                        {
                            if (latestDateItem.PublishDate.Year < 2020)
                            {
                                lastUpdatedTime = DateTimeOffset.Now;
                            }
                            else
                            {
                                lastUpdatedTime = latestDateItem.PublishDate;
                            }
                        }
                    }
                    else
                    {
                        lastUpdatedTime = new DateTimeOffset(2020, 10, 30, 20, 00, 00, new TimeSpan(2, 0, 0));
                    }
                }
                return (true, lastUpdatedTime, rssItemsCount);
            }
        }

        /// <summary>
        /// Get source info from rss url
        /// </summary>
        /// <param name="source">string for rss/xml content</param>        
        /// <param name="rssUrl">string for source rss url</param>
        /// <returns>Task Source with all of its info or null of there is a problem</returns>
        public async Task<Source?> GetSourceInfoFromRssAsync(string? source, string? rssUrl)
        {
            if (string.IsNullOrEmpty(source))
            {
                return null;
            }

            if (string.IsNullOrEmpty(rssUrl))
            {
                return null;
            }

            var userAgent = await new UserAgentService().GetCurrentAgentAsync();

            return await Task.Run(() =>
            {
                using XmlReader xmlReader = XmlReader.Create(new StringReader(source), new XmlReaderSettings { Async = true, IgnoreWhitespace = true, IgnoreComments = true });
                SyndicationFeed feed = SyndicationFeed.Load(xmlReader);
                var newSource = new Source
                {
                    SiteTitle = feed.Title.Text,
                    Description = feed.Description.Text,
                    Language = feed.Language,
                    LastBuildCheck = feed.LastUpdatedTime,
                    LastBuildDate = feed.LastUpdatedTime,
                    RssUrl = new Uri(rssUrl),
                    IsWorking = true
                };

                // try to get the base url for the feed from the rss xml
                foreach (var link in feed.Links.Where(x => x.MediaType is null))
                {
                    if (link.Uri is not null)
                    {
                        if (link.BaseUri is null)
                        {
                            newSource.BaseUrl = link.Uri;
                        }
                        else
                        {
                            newSource.BaseUrl = link.BaseUri;
                        }
                        break;
                    }
                }

                // try to get the base url for the feed from the rss url
                if (newSource.BaseUrl is null || newSource.BaseUrl == new Uri("about:blank"))
                {
                    newSource.BaseUrl = new Uri(new Uri(rssUrl).Host);
                }

                // try to get the favicon for the site via duckduckgo service
                System.Net.WebClient webClient = new();
                if (!string.IsNullOrEmpty(userAgent.AgentString))
                {
                    webClient.Headers.Add("User-Agent", userAgent.AgentString);
                }
                newSource.SiteIcon = webClient.DownloadData($"https://icons.duckduckgo.com/ip3/{newSource.BaseUrl.Host}.ico");

                return newSource;
            });
        }

        public async Task<Source> AddNewSourceAsync(Source source)
        {
            return await Task.Run(() =>
            {
                using var db = new LiteDatabase(LiteDbContext.DbConnectionString);
                var col = db.GetCollection<Source>(LiteDbContext.Sources);
                col.Insert(source);
                return source;
            });
        }

        public async Task<Source> UpdateSourceAsync(Source source)
        {
            return await Task.Run(() =>
            {
                using var db = new LiteDatabase(LiteDbContext.DbConnectionString);
                var col = db.GetCollection<Source>(LiteDbContext.Sources);
                col.Update(source);
                return source;
            });
        }

        public async Task<bool> DeleteSourceAsync(Source source)
        {
            return await Task.Run(() =>
            {
                using var db = new LiteDatabase(LiteDbContext.DbConnectionString);
                return db.GetCollection<Source>(LiteDbContext.Sources).Delete(source.Id);
            });
        }
    }
}
