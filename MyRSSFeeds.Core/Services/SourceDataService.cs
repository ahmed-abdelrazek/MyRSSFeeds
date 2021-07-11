using LiteDB;
using MyRSSFeeds.Core.Data;
using MyRSSFeeds.Core.Helpers;
using MyRSSFeeds.Core.Models;
using MyRSSFeeds.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;

namespace MyRSSFeeds.Core.Services
{
    public class SourceDataService : ISourceDataService
    {
        public async Task<IEnumerable<Source>> GetSourcesDataAsync()
        {
            return await Task.Run(() =>
            {
                using (var db = new LiteDatabase(LiteDbContext.DbConnectionString))
                {
                    return db.GetCollection<Source>(LiteDbContext.Sources).FindAll().OrderByDescending(x => x.Id);
                }
            });
        }

        public async Task<IEnumerable<Source>> GetSourcesDataWithFeedsAsync()
        {
            return await Task.Run(() =>
            {
                using (var db = new LiteDatabase(LiteDbContext.DbConnectionString))
                {
                    return db.GetCollection<Source>(LiteDbContext.Sources).Include(x => x.RSSs).FindAll();
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
                var col = db.GetCollection<Source>(LiteDbContext.Sources);
                return col.Exists(x => x.RssUrl == link);
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

            return await Task.Run(() =>
            {
                using XmlReader xmlReader = XmlReader.Create(new StringReader(source), new XmlReaderSettings { Async = true, IgnoreWhitespace = true, IgnoreComments = true });
                SyndicationFeed feed = SyndicationFeed.Load(xmlReader);
                Uri baseLink = new Uri("");
                foreach (var link in feed.Links.Where(x => x.MediaType is null))
                {
                    if (link.Uri is not null)
                    {
                        baseLink = link.BaseUri;
                        break;
                    }
                }

                return new Source
                {
                    SiteTitle = feed.Title.Text,
                    Description = feed.Description.Text,
                    Language = feed.Language,
                    LastBuildDate = feed.LastUpdatedTime,
                    BaseUrl = baseLink,
                    RssUrl = new Uri(rssUrl),
                    IsWorking = true
                };
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
